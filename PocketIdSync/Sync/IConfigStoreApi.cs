using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.ModelSpecs;

namespace PocketIdSync.Sync;

interface IConfigStoreApi
{
    bool Exists { get; }

    Task<int> LoadAsync(List<ApiSyncItem> apis, CancellationToken ct = default);
    Task<int> LoadAsync(List<ApiSyncItem> apis, string filename, string? ns = null, CancellationToken ct = default);
    Task<int> SynchronizeAsync(ApiSyncItem api, CancellationToken ct = default);
    Task<int> WriteAsync<T>(ApiSyncItem api, T? data, CancellationToken ct = default) where T : IKubernetes;
}
