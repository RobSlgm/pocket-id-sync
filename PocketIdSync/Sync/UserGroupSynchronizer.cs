using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.Apis;
using PocketIdSync.ModelSpecs;

namespace PocketIdSync.Sync;

internal sealed class UserGroupSynchronizer : ISynchronizer<UserGroupSyncItem>
{
    public List<UserGroupSyncItem> Items { get; private set; } = [];
    private readonly IConfigStoreUserGroup Configuration;

    public UserGroupSynchronizer(IConfigStoreUserGroup configuration)
    {
        Configuration = configuration;
    }

    public async Task<(int ExitCode, UserGroupSyncItem? Client, string? ErrorMessage)> CombineAsync(PocketIdClient pocketId, SynchronizationTarget direction, SyncItemSelector? selector, CancellationToken ct)
    {
        return direction == SynchronizationTarget.PocketID ?
          await MergeFromPocketIdAsync(pocketId, ct) :
          await MergeFromConfigurationAsync(pocketId, selector, ct);
    }


    private async Task<(int ExitCode, UserGroupSyncItem? Client, string? ErrorMessage)> MergeFromPocketIdAsync(PocketIdClient pocketId, CancellationToken ct)
    {
        var allShortResponse = await pocketId.UserGroups.ListAsync(ct);
        if (!allShortResponse.IsSuccessful)
        {
            return (ExitCode.FatalError, default, default);
        }
        foreach (var group in Items)
        {
            var groupSource = allShortResponse.Data?.FirstOrDefault(g => string.Equals(group.Name, g.Name, StringComparison.OrdinalIgnoreCase));
            if (groupSource is not null)
            {
                group.Remote = UserGroupMapper.MapInto(groupSource);
                group.Id = groupSource.Id;
            }
        }
        return (ExitCode.Success, default, default);
    }

    private async Task<(int ExitCode, UserGroupSyncItem? Client, string? ErrorMessage)> MergeFromConfigurationAsync(PocketIdClient pocketId, SyncItemSelector? selector, CancellationToken ct)
    {
        var allShortResponse = await pocketId.UserGroups.ListAsync(ct);
        if (!allShortResponse.IsSuccessful)
        {
            return (ExitCode.FatalError, default, default);
        }
        foreach (var clientShort in allShortResponse.Data ?? [])
        {
            if (string.IsNullOrEmpty(clientShort.Id)) continue;
            var client = new UserGroupSyncItem
            {
                Id = clientShort.Id,
                Name = clientShort.Name,
            };
            if (!client.IsMatch(selector))
            {
                continue;
            }
            var clientResponse = await pocketId.UserGroups.Id(clientShort.Id).GetAsync(ct);
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
        foreach (var group in Items.Where(c => (c.IsRemoteEqualLocal == false) && c.HasError == false))
        {
            if (group.Local?.Spec is null) continue;
            var userGroup = group.Local.Spec.FromKind(group.Remote);
            if (group.IsRemoteEqualLocal == false)
            {
                if (group.Remote is not null)
                {
                    var update = await pocketId.UserGroups.Id(userGroup.Id!).PutAsync(userGroup, ct);
                    if (!update.IsSuccessful)
                    {
                        group.SetError(update.errorMessage);
                        exitCode = ExitCode.GeneralError;
                        continue;
                    }
                    group.RemoteMerged = update.Data;
                }
                else
                {
                    var create = await pocketId.UserGroups.PostAsync(userGroup, ct);
                    if (!create.IsSuccessful)
                    {
                        group.SetError(create.errorMessage);
                        exitCode = ExitCode.GeneralError;
                        continue;
                    }
                    group.RemoteMerged = create.Data;
                }
            }
        }
        return exitCode;
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
        if (selector is null)
        {
            return await Configuration.LoadAsync(Items, ct);
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
            var loadAllResponse = await Configuration.LoadAsync(Items, ct);
            if (loadAllResponse != ExitCode.Success)
            {
                return loadAllResponse;
            }
        }

        if (Items.Count == 0)
        {
            // AnsiConsole.MarkupLine($"[bold red]✗ No valid content found in {StoreRoot.FullName}[/]");
            return ExitCode.BadRequest;
        }

        if (!string.IsNullOrEmpty(selector.Name))
        {
            Items.RemoveAll(c => string.IsNullOrEmpty(c.Name) || !c.Name.Equals(selector.Name, StringComparison.OrdinalIgnoreCase));
            if (Items.Count == 0)
            {
                // AnsiConsole.MarkupLine($"[bold red]✗ No valid configuration with the name {Name} found[/]");
                return ExitCode.InvalidConfiguration;
            }
        }
        foreach (var group in Items)
        {
            if (group.Local is null || group.Local.Spec is null)
            {
                group.SetError($"Local specification missing or invalid");
                continue;
            }
            var local = group.Local.Spec;
            if (local is null) continue;
        }
        int removeFailed = Items.RemoveAll(c => c.Local is null || c.Local.Spec is null || c.HasError == true);
        return removeFailed == 0 ? ExitCode.Success : ExitCode.GeneralError;
    }
}
