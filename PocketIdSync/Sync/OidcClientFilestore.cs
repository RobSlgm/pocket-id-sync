using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.Apis;
using PocketIdSync.ModelSpecs;
using PocketIdSync.Utils;

namespace PocketIdSync.Sync;

sealed class OidcClientFilestore : IConfigStoreOidcClient
{
    private readonly DirectoryInfo Root;
    private readonly YamlHelper Yaml;

    public OidcClientFilestore(string rootPath, YamlHelper yamlHelper)
    {
        Root = new DirectoryInfo(Path.Combine(rootPath, "default", "oidcClient"));
        Yaml = yamlHelper;
    }

    public bool UseSidecars { get; set; } = false;

    public bool Exists
    {
        get
        {
            return Root.Exists;
        }
    }

    public async Task<int> LoadAsync(List<OidcClientSyncItem> clients, bool existingOnly = false, CancellationToken ct = default)
    {
        foreach (var specFile in Root.EnumerateFiles("*.yaml"))
        {
            try
            {
                if (specFile.FullName.Contains(".Secret.", StringComparison.OrdinalIgnoreCase)) continue;
                var client = new OidcClientSyncItem { Filename = specFile.FullName };
                var readClient = await ReadFileAsync(client, ct);
                if (readClient is null || client.HasError == true)
                {
                    clients.Add(client);
                    continue;
                }
                var existing = clients.FirstOrDefault(c => string.Equals(c.Id, client.Id, StringComparison.OrdinalIgnoreCase));
                if (existing is not null)
                {
                    existing.Filename = client.Filename;
                    existing.Local = client.Local;
                }
                else
                {
                    if (existingOnly != true)
                    {
                        clients.Add(client);
                    }
                }
            }
            catch
            {
                return ExitCode.FatalError;
            }
        }
        return ExitCode.Success;
    }

    public async Task<int> LoadAsync(List<OidcClientSyncItem> clients, string filename, string? ns = null, CancellationToken ct = default)
    {
        try
        {
            FileInfo specFile = new FileInfo(Path.Combine(Root.FullName, filename));
            if (!specFile.Exists && string.IsNullOrEmpty(specFile.Extension))
            {
                specFile = new FileInfo(Path.Combine(Root.FullName, $"{filename}.yaml"));
            }
            if (!specFile.Exists)
            {
                return ExitCode.BadRequest;
            }
            var client = new OidcClientSyncItem { Filename = specFile.FullName };
            await ReadFileAsync(client, ct);
            clients.Add(client);
            return ExitCode.Success;
        }
        catch
        {
            return ExitCode.FatalError;
        }
    }

    private async Task<OidcClientSyncItem?> ReadFileAsync(OidcClientSyncItem client, CancellationToken ct = default)
    {
        try
        {
            var oidcClientKind = await Yaml.ReadAsync<OidcClientKind>(client.Filename!, ct);
            if (oidcClientKind is not null)
            {
                client.Namespace = oidcClientKind.Metadata?.Namespace ?? client.Namespace ?? "default";
                client.Name = oidcClientKind.Metadata?.Name;
                client.Id = oidcClientKind.Spec?.Id;
                if (!string.IsNullOrEmpty(client.Namespace) && !string.IsNullOrEmpty(client.Name) && !string.IsNullOrEmpty(client.Id))
                {
                    client.Local = oidcClientKind;
                }
            }
            return client;
        }
        catch (Exception e)
        {
            client.SetError($"{client.Filename}: {e.Message}");
            return default;
        }
    }

    public async Task<int> SynchronizeAsync(OidcClientSyncItem client, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(client.Filename))
        {
            FileInfo specFile = new FileInfo(Path.Combine(Root.FullName, $"{client.Id}.yaml"));
            client.Filename = specFile.FullName;
        }
        client.LocalMerged = client.Remote?.ToKind(client.Local);
        client.IsLocalDirty = !client.IsRemoteEqualLocal;
        return ExitCode.Success;
    }

    public async Task<int> WriteAsync<T>(OidcClientSyncItem client, T? data, CancellationToken ct = default) where T : IKubernetes
    {
        try
        {
            if (data is null || string.IsNullOrEmpty(client.Filename)) return ExitCode.BadRequest;
            var name = !string.Equals(data.Kind, "OidcClient", StringComparison.OrdinalIgnoreCase) ? $"{client.Id}.{data.Kind}.yaml" : $"{client.Id}.yaml";
            var fileName = new FileInfo(client.Filename);
            var yamlPath = Path.Combine(fileName.DirectoryName!, name);
            var content = Yaml.Write(data);
            var utf8Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            await File.WriteAllTextAsync(yamlPath, content, utf8Encoding, ct);
            return ExitCode.Success;
        }
        catch
        {
            return ExitCode.FatalError;
        }
    }

    public async Task<int> WriteLogoAsync(OidcClientSyncItem client, byte[] data, string filename, LogoThemeMode theme, CancellationToken ct = default)
    {
        try
        {
            if (client.LocalMerged?.Spec is not null)
            {
                if (theme == LogoThemeMode.Light)
                {
                    client.LocalMerged.Spec.LogoPath = filename;
                    if (!UseSidecars)
                    {
                        client.LocalMerged.Spec.LogoContent = data;
                    }
                }
                else
                {
                    client.LocalMerged.Spec.LogoDarkPath = filename;
                    if (!UseSidecars)
                    {
                        client.LocalMerged.Spec.LogoDarkContent = data;
                    }
                }
            }
            if (UseSidecars)
            {
                var logoPath = Path.Combine(Root.FullName, filename);
                FileInfo specFile = new FileInfo(logoPath);
                await File.WriteAllBytesAsync(specFile.FullName, data, ct);
            }
            return ExitCode.Success;
        }
        catch
        {
            return ExitCode.FatalError;
        }
    }

    public async Task<ConfigStoreFileResult> ReadLogoAsync(OidcClientSyncItem client, LogoThemeMode theme, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(client.Filename)) return new(ExitCode.BadRequest, default, default, default);
            if (client.Local?.Spec is null) return new(ExitCode.BadRequest, default, default, default);
            var spec = client.Local.Spec;

            var logoFilename = theme == LogoThemeMode.Light ? spec.LogoPath : spec.LogoDarkPath;
            if (string.IsNullOrEmpty(logoFilename)) return new(ExitCode.BadRequest, default, default, default);
            var mimetype = MimeTypeUtil.ToMimeType(new FileInfo(logoFilename).Extension);

            var logoContent = theme == LogoThemeMode.Light ? spec.LogoContent : spec.LogoDarkContent;
            if (logoContent is not null && logoContent.Length > 0)
            {
                return new(ExitCode.Success, logoContent, mimetype, logoFilename);
            }
            var clientYaml = new FileInfo(client.Filename);
            var logoPath = new FileInfo(Path.Combine(clientYaml.DirectoryName!, logoFilename));
            if (!logoPath.Exists) return new(ExitCode.BadRequest, default, default, logoFilename);
            var content = await File.ReadAllBytesAsync(logoPath.FullName, ct);
            return new(ExitCode.Success, content, mimetype, logoFilename);
        }
        catch
        {
            return new(ExitCode.FatalError, default, default, default);
        }
    }
}
