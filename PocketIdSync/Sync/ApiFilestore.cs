using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.ModelSpecs;
using PocketIdSync.Utils;

namespace PocketIdSync.Sync;

sealed class ApiFilestore : IConfigStoreApi
{
    private readonly DirectoryInfo Root;
    private readonly YamlHelper Yaml;

    public ApiFilestore(string rootPath, YamlHelper yamlHelper)
    {
        Root = Directory.CreateDirectory(Path.Combine(rootPath, "oidcclientapi"));
        Yaml = yamlHelper;
    }

    public bool Exists
    {
        get
        {
            return Root.Exists;
        }
    }

    public async Task<int> LoadAsync(List<ApiSyncItem> apis, CancellationToken ct = default)
    {
        foreach (var specFile in Root.EnumerateFiles("*.yaml"))
        {
            try
            {
                var sync = new ApiSyncItem { Filename = specFile.FullName };
                var readApi = await ReadFileAsync(sync, ct);
                if (readApi is null)
                {
                    continue;
                }
                var existing = apis.FirstOrDefault(c => string.Equals(c.Name, sync.Name, StringComparison.OrdinalIgnoreCase));
                if (existing is not null)
                {
                    existing.Filename = sync.Filename;
                    existing.Local = sync.Local;
                }
                else
                {
                    apis.Add(sync);
                }
            }
            catch
            {
                return ExitCode.FatalError;
            }
        }
        return ExitCode.Success;
    }

    public async Task<int> LoadAsync(List<ApiSyncItem> apis, string filename, string? ns = null, CancellationToken ct = default)
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
            var client = new ApiSyncItem { Filename = specFile.FullName };
            await ReadFileAsync(client, ct);
            apis.Add(client);
            return ExitCode.Success;
        }
        catch
        {
            return ExitCode.FatalError;
        }
    }

    private async Task<ApiSyncItem?> ReadFileAsync(ApiSyncItem client, CancellationToken ct = default)
    {
        try
        {
            var kind = await Yaml.ReadAsync<OidcClientApiKind>(client.Filename!, ct);
            if (kind is not null)
            {
                client.Namespace = kind.Metadata?.Namespace ?? client.Namespace ?? "default";
                client.Name = kind.Metadata?.Name;
                // client.Id = kind.Spec?.Name;
                // client.Id = kind.Spec?.Id;
                if (!string.IsNullOrEmpty(client.Namespace) && !string.IsNullOrEmpty(client.Name))
                {
                    client.Local = kind;
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

    public async Task<int> SynchronizeAsync(ApiSyncItem api, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(api.Filename))
        {
            FileInfo specFile = new FileInfo(Path.Combine(Root.FullName, $"{api.Id}.yaml"));
            api.Filename = specFile.FullName;
        }
        var ns = api.Local is null ? "default" : api.Namespace;
        var kind = api.Remote?.ToKind(ns);
        api.LocalMerged = kind;
        api.IsLocalDirty = !api.IsRemoteEqualLocal;
        return ExitCode.Success;
    }

    public async Task<int> WriteAsync<T>(ApiSyncItem api, T? data, CancellationToken ct = default) where T : IKubernetes
    {
        try
        {
            if (data is null || string.IsNullOrEmpty(api.Filename)) return ExitCode.BadRequest;
            var name = !string.Equals(data.Kind, "OidcClientApi", StringComparison.OrdinalIgnoreCase) ? $"{api.Id}.{data.Kind}.yaml" : $"{api.Id}.yaml";
            var fileName = new FileInfo(api.Filename);
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
}
