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

sealed class UserGroupFilestore : IConfigStoreUserGroup
{
    private readonly DirectoryInfo Root;
    private readonly YamlHelper Yaml;

    public UserGroupFilestore(string rootPath, YamlHelper yamlHelper)
    {
        Root = Directory.CreateDirectory(Path.Combine(rootPath, "usergroup"));
        Yaml = yamlHelper;
    }

    public bool Exists
    {
        get
        {
            return Root.Exists;
        }
    }

    public async Task<int> LoadAsync(List<UserGroupSyncItem> clients, CancellationToken ct = default)
    {
        foreach (var specFile in Root.EnumerateFiles("*.yaml"))
        {
            try
            {
                var client = new UserGroupSyncItem { Filename = specFile.FullName };
                var readClient = await ReadFileAsync(client, ct);
                if (readClient is null)
                {
                    continue;
                }
                var existing = clients.FirstOrDefault(c => string.Equals(c.Name, client.Name, StringComparison.OrdinalIgnoreCase));
                if (existing is not null)
                {
                    existing.Filename = client.Filename;
                    existing.Local = client.Local;
                }
                else
                {
                    clients.Add(client);
                }
            }
            catch
            {
                return ExitCode.FatalError;
            }
        }
        return ExitCode.Success;
    }

    public async Task<int> LoadAsync(List<UserGroupSyncItem> clients, string filename, string? ns = null, CancellationToken ct = default)
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
            var client = new UserGroupSyncItem { Filename = specFile.FullName };
            await ReadFileAsync(client, ct);
            clients.Add(client);
            return ExitCode.Success;
        }
        catch
        {
            return ExitCode.FatalError;
        }
    }

    private async Task<UserGroupSyncItem?> ReadFileAsync(UserGroupSyncItem client, CancellationToken ct = default)
    {
        try
        {
            var kind = await Yaml.ReadAsync<UserGroupKind>(client.Filename!, ct);
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

    public async Task<int> SynchronizeAsync(UserGroupSyncItem client, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(client.Filename))
        {
            FileInfo specFile = new FileInfo(Path.Combine(Root.FullName, $"{client.Id}.yaml"));
            client.Filename = specFile.FullName;
        }
        var ns = client.Local is null ? "default" : client.Namespace;
        var kind = client.Remote?.ToKind(ns);
        client.LocalMerged = kind;
        client.IsLocalDirty = !client.IsRemoteEqualLocal;
        return ExitCode.Success;
    }

    public async Task<int> WriteAsync<T>(UserGroupSyncItem client, T? data, CancellationToken ct = default) where T : IKubernetes
    {
        try
        {
            if (data is null || string.IsNullOrEmpty(client.Filename)) return ExitCode.BadRequest;
            var name = !string.Equals(data.Kind, "UserGroup", StringComparison.OrdinalIgnoreCase) ? $"{client.Id}.{data.Kind}.yaml" : $"{client.Id}.yaml";
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
}
