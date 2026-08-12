using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.Apis;
using PocketIdSync.Models;
using PocketIdSync.ModelSpecs;
using PocketIdSync.Repositories;
using PocketIdSync.Utils;

namespace PocketIdSync.Sync;

sealed class OidcClientSynchronizer : ISynchronizer<OidcClientSyncItem>
{
    public List<OidcClientSyncItem> Items { get; private set; } = [];
    private Dictionary<string, UserGroupMinimalDto> UserGroups { get; } = new Dictionary<string, UserGroupMinimalDto>(StringComparer.OrdinalIgnoreCase);
    private readonly IConfigStoreOidcClient Configuration;
    private readonly OidcClientRepository OidcClientRepository = new();

    public bool ForceLogoSynchronization { get; set; }

    public OidcClientSynchronizer(IConfigStoreOidcClient configuration)
    {
        Configuration = configuration;
    }

    private void AddUserGroups(UserGroupMinimalDto[] userGroups)
    {
        foreach (var ug in userGroups ?? [])
        {
            UserGroups.TryAdd(ug.Name ?? ug.Id!, ug);
        }
    }

    public async Task<int> LoadUserGroupsAsync(PocketIdClient pocketId, CancellationToken ct)
    {
        var userGroupResponse = await pocketId.UserGroups.ListAsync(ct);
        if (!userGroupResponse.IsSuccessful)
        {
            return ExitCode.FatalError;
        }
        AddUserGroups(userGroupResponse.Data ?? []);
        return ExitCode.Success;
    }

    public async Task<(int ExitCode, OidcClientSyncItem? Client, string? ErrorMessage)> CombineAsync(PocketIdClient pocketId, SynchronizationTarget direction, SyncItemSelector? selector, CancellationToken ct)
    {
        var userGroupResult = await LoadUserGroupsAsync(pocketId, ct);
        if (userGroupResult != ExitCode.Success)
        {
            return (ExitCode.FatalError, default, "UserGroups");
        }
        return direction == SynchronizationTarget.PocketID ?
          await MergeFromPocketIdAsync(pocketId, ct) :
          await MergeFromConfigurationAsync(pocketId, selector, ct);
    }


    private async Task<(int ExitCode, OidcClientSyncItem? Client, string? ErrorMessage)> MergeFromPocketIdAsync(PocketIdClient pocketId, CancellationToken ct)
    {
        foreach (var client in Items)
        {
            var clientSource = await OidcClientRepository.GetAsync(pocketId, client.Id!, ct);
            if (clientSource.IsSuccessful)
            {
                client.Remote = clientSource.Data;
            }
            else
            {
                if (clientSource.Status != System.Net.HttpStatusCode.NotFound)
                {
                    return (ExitCode.FatalError, client, $"{clientSource.Status}");
                }
            }
            if (client.Local?.Spec?.AllowedGroups is not null && client.Local.Spec.AllowedGroups.Length > 0)
            {
                foreach (var groupName in client.Local.Spec.AllowedGroups)
                {
                    if (!UserGroups.TryGetValue(groupName, out var ug))
                    {
                        return (ExitCode.BadRequest, client, $"UserGroup {groupName} not found");
                    }
                }
            }
        }
        return (ExitCode.Success, default, default);
    }

    private async Task<(int ExitCode, OidcClientSyncItem? Client, string? ErrorMessage)> MergeFromConfigurationAsync(PocketIdClient pocketId, SyncItemSelector? selector, CancellationToken ct)
    {
        var allShortResponse = await pocketId.OidcClients.ListAsync(ct);
        if (!allShortResponse.IsSuccessful)
        {
            return (ExitCode.FatalError, default, default);
        }
        foreach (var clientShort in allShortResponse.Data ?? [])
        {
            if (string.IsNullOrEmpty(clientShort.Id)) continue;
            var client = new OidcClientSyncItem
            {
                Id = clientShort.Id,
                Name = clientShort.Name,
            };
            if (!client.IsMatch(selector))
            {
                continue;
            }
            var clientResponse = await OidcClientRepository.GetAsync(pocketId, clientShort.Id, ct);
            if (!clientResponse.IsSuccessful)
            {
                return (ExitCode.FatalError, client, $"Merge failed {clientResponse.Status}");
            }
            client.Remote = clientResponse.Data;
            Items.Add(client);
        }
        return (ExitCode.Success, default, default);
    }

    public async Task<int> SynchronizeAsync(PocketIdClient pocketId, SynchronizationTarget direction, CancellationToken ct)
    {
        return direction switch
        {
            SynchronizationTarget.PocketID => await SynchronizeToPocketIdAsync(pocketId, ct),
            SynchronizationTarget.Configuration => await SynchronizeToConfigurationAsync(pocketId, ct),
            _ => ExitCode.Unauthorized,
        };
    }

    private async Task<int> SynchronizeToPocketIdAsync(PocketIdClient pocketId, CancellationToken ct)
    {
        var exitCode = ExitCode.Success;
        foreach (var client in Items.Where(c => (c.IsRemoteEqualLocal == false || ForceLogoSynchronization == true) && c.HasError == false))
        {
            if (client.Local?.Spec is null) continue;
            if (client.IsRemoteEqualLocal == false)
            {
                var amendResponse = await OidcClientRepository.AmendAsync(pocketId, client.Remote?.Id, client.Local.Spec, ct);
                if (amendResponse is null || !amendResponse.IsSuccessful || amendResponse.Data is null)
                {
                    client.SetError(amendResponse?.ErrorMessage);
                    exitCode = ExitCode.GeneralError;
                    continue;
                }
                client.RemoteMerged = amendResponse.Data;
                if (client.Remote is null && client.RemoteMerged.IsPublic == false)
                {
                    var secret = await pocketId.OidcClients.Id(client.RemoteMerged.Id!).SetSecretAsync(ct);
                    if (!secret.IsSuccessful)
                    {
                        client.SetError($"Secret: {secret.ErrorMessage}");
                        exitCode = ExitCode.GeneralError;
                        continue;
                    }
                    if (secret.Data is not null && !string.IsNullOrEmpty(secret.Data.Secret))
                    {
                        client.Secret = secret.Data.Secret;
                        var secretResponse = await WriteSecret(client, secret.Data.Secret, ct);
                        if (secretResponse != ExitCode.Success)
                        {
                            client.SetError("Secret not retrieved");
                            exitCode = secretResponse;
                            continue;
                        }
                    }
                }
            }
            foreach (var theme in Enum.GetValues<LogoThemeMode>())
            {
                var logo = await LogoToPocketIdAsync(pocketId, client, theme, ct);
                if (logo != ExitCode.Success)
                {
                    exitCode = ExitCode.GeneralError;
                    break;
                }
            }
        }
        return exitCode;
    }

    private async Task<int> WriteSecret(OidcClientSyncItem client, string secret, CancellationToken ct)
    {
        var kind = new OidcClientSecretKind
        {
            Kind = "Secret",
            ApiVersion = "v1",
            Metadata = new KubernetesMetadata
            {
                Name = client.Name,
                Namespace = client.Namespace,
            },
            Data = new OidcClientSecretSpec
            {
                ClientId = client.Id,
                ClientSecret = secret,
            },
        };
        return await Configuration.WriteAsync(client, kind, ct);
    }

    private async Task<int> SynchronizeToConfigurationAsync(PocketIdClient pocketId, CancellationToken ct)
    {
        foreach (var client in Items.Where(c => c.Remote is not null && c.HasError == false))
        {
            var syncResponse = await Configuration.SynchronizeAsync(client, ct);
            if (syncResponse != ExitCode.Success)
            {
                client.SetError("Sync");
                continue;
            }
            if (client.Remote is not null && (client.IsRemoteEqualLocal == false || ForceLogoSynchronization == true))
            {
                foreach (var theme in Enum.GetValues<LogoThemeMode>())
                {
                    var hasLogo = theme == LogoThemeMode.Light ? client.Remote.HasLogo == true : client.Remote.HasDarkLogo == true;
                    if (hasLogo == true)
                    {
                        var logoResponse = await pocketId.OidcClients.Id(client.Id!).Logo(theme).GetAsync(ct);
                        if (!logoResponse.IsSuccessful || logoResponse.Data is null)
                        {
                            client.SetError($"Download {theme} mode logo: {logoResponse.Status}");
                            continue;
                        }
                        var extension = MimeTypeUtil.FromMimeType(logoResponse.MimeType) ?? ".unknown";
                        var filename = $"{client.Id}{(theme == LogoThemeMode.Light ? "" : "-dark")}{extension}";

                        if (client.Local is not null)
                        {
                            var localLogo = await Configuration.ReadLogoAsync(client, theme, ct);
                            if (localLogo.IsSuccessful)
                            {
                                var isSameContent = new Span<byte>(logoResponse.Data).SequenceEqual(new Span<byte>(localLogo.Content));
                                if (isSameContent == true)
                                {
                                    if (!localLogo.isSidecar && client.LocalMerged is not null)
                                    {
                                        if (theme == LogoThemeMode.Light)
                                        {
                                            client.LocalMerged.Spec?.LogoContent = localLogo.Content;
                                        }
                                        else
                                        {
                                            client.LocalMerged.Spec?.LogoDarkContent = localLogo.Content;
                                        }
                                    }
                                    continue;   // nothing to do, logo is not changed
                                }
                            }
                        }
                        client.IsLocalDirty = true; // Force writeing of YAML, logo is changed or new
                        var writeResponse = await Configuration.WriteLogoAsync(client, logoResponse.Data, filename, theme, ct);
                        if (writeResponse != ExitCode.Success)
                        {
                            client.SetError($"Write local {theme} mode logo to {filename}");
                            break;
                        }
                    }
                }
            }
            if (client.HasError == false && client.LocalMerged is not null && client.IsLocalDirty == true)
            {
                var store = await Configuration.WriteAsync(client, client.LocalMerged, ct);
                if (store != ExitCode.Success)
                {
                    client.SetError("Local store write");
                    continue;
                }
            }
        }
        return ExitCode.Success;
    }

    public async Task<int> LoadConfiguration(SyncItemSelector? selector, CancellationToken ct)
    {
        if (selector is null || !selector.IsRestricted)
        {
            return await Configuration.LoadAsync(Items, selector is null, ct: ct);
        }
        if (!string.IsNullOrEmpty(selector.Filename))
        {
            var loadFileResponse = await Configuration.LoadAsync(Items, selector.Filename, ns: null, ct);
            if (loadFileResponse != ExitCode.Success)
            {
                return ExitCode.InvalidConfiguration;
            }
        }
        else
        {
            var loadAllResponse = await Configuration.LoadAsync(Items, ct: ct);
            if (loadAllResponse != ExitCode.Success)
            {
                return loadAllResponse;
            }
        }

        if (Items.Count == 0)
        {
            return ExitCode.BadRequest;
        }

        if (!string.IsNullOrEmpty(selector.Name))
        {
            Items.RemoveAll(c => string.IsNullOrEmpty(c.Name) || !c.Name.Equals(selector.Name, StringComparison.OrdinalIgnoreCase));
            if (Items.Count == 0)
            {
                return ExitCode.InvalidConfiguration;
            }
        }
        if (!string.IsNullOrEmpty(selector.Id))
        {
            Items.RemoveAll(c => string.IsNullOrEmpty(c.Id) || !c.Id.Equals(selector.Id, StringComparison.OrdinalIgnoreCase));
            if (Items.Count == 0)
            {
                return ExitCode.InvalidConfiguration;
            }
        }
        foreach (var client in Items)
        {
            if (client.Local is null || client.Local.Spec is null)
            {
                client.SetError($"Local specification missing or invalid");
                continue;
            }
            var local = client.Local.Spec;
            if (local is null) continue;
            foreach (var theme in Enum.GetValues<LogoThemeMode>())
            {
                var logoPath = theme == LogoThemeMode.Light ? local.LogoPath : local.LogoDarkPath;
                if (!string.IsNullOrEmpty(logoPath))
                {
                    var logoResponse = await Configuration.ReadLogoAsync(client, theme, ct);
                    if (!logoResponse.IsSuccessful)
                    {
                        client.SetError($"Missing {theme} mode Logo {logoPath}");
                        break;
                    }
                }
            }
        }
        int removeFailed = Items.RemoveAll(c => c.Local is null || c.Local.Spec is null || c.HasError == true);
        return removeFailed == 0 ? ExitCode.Success : ExitCode.GeneralError;
    }

    private async Task<int> LogoToPocketIdAsync(PocketIdClient pocketId, OidcClientSyncItem client, LogoThemeMode theme, CancellationToken ct)
    {
        var hasLocalLogo = theme == LogoThemeMode.Light ? !string.IsNullOrEmpty(client.Local?.Spec?.LogoPath) : !string.IsNullOrEmpty(client.Local?.Spec?.LogoDarkPath);
        var hasRemoteLogo = theme == LogoThemeMode.Light ? client.Remote?.HasLogo == true : client.Remote?.HasDarkLogo == true;
        if (hasLocalLogo == true)
        {
            var localLogo = await Configuration.ReadLogoAsync(client, theme, ct);
            if (localLogo.ExitCode != ExitCode.Success || localLogo.Content is null)
            {
                return localLogo.ExitCode;
            }
            var remoteLogoResponse = await pocketId.OidcClients.Id(client.Id!).Logo(theme).GetAsync(ct);
            if (remoteLogoResponse.IsSuccessful && remoteLogoResponse.Data is not null)
            {
                var isSameContent = new Span<byte>(remoteLogoResponse.Data).SequenceEqual(new Span<byte>(localLogo.Content));
                if (isSameContent == true)
                {
                    return ExitCode.Success;    // nothing to do
                }
            }
            // pocketId.Refresh();
            var logoUploadResponse = await pocketId.OidcClients.Id(client.Id!).Logo(theme).AmendAsync(localLogo.Content, localLogo.Filename!, localLogo.Mimetype!, ct);
            if (!logoUploadResponse.IsSuccessful)
            {
                client.SetError($"Upload {theme} mode logo");
                return ExitCode.FatalError;
            }
            return ExitCode.Success;
        }
        if (hasRemoteLogo == true)
        {
            var deleteResponse = await pocketId.OidcClients.Id(client.Id!).Logo(theme).DeleteAsync(ct);
            if (!deleteResponse.IsSuccessful)
            {
                client.SetError($"Delete {theme} mode logo: {deleteResponse.Status}");
                //return ExitCode.FatalError;
            }
            return ExitCode.Success;
        }
        return ExitCode.Success;
    }
}
