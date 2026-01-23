using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.ModelSpecs;

namespace PocketIdSync.Sync;

interface IConfigStoreUserGroup
{
    bool Exists { get; }

    Task<int> LoadAsync(List<UserGroupSyncItem> clients, CancellationToken ct = default);
    Task<int> LoadAsync(List<UserGroupSyncItem> clients, string filename, string? ns = null, CancellationToken ct = default);
    Task<int> SynchronizeAsync(UserGroupSyncItem client, CancellationToken ct = default);
    Task<int> WriteAsync<T>(UserGroupSyncItem client, T? data, CancellationToken ct = default) where T : IKubernetes;
}
