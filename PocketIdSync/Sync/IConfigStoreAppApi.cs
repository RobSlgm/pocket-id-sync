using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.ModelSpecs;

namespace PocketIdSync.Sync;

interface IConfigStoreAppApi
{
    bool Exists { get; }

    Task<int> LoadAsync(List<AppApiSyncItem> appapis, CancellationToken ct = default);
    Task<int> LoadAsync(List<AppApiSyncItem> appapis, string filename, string? ns = null, CancellationToken ct = default);
    Task<int> SynchronizeAsync(AppApiSyncItem appapi, CancellationToken ct = default);
    Task<int> WriteAsync<T>(AppApiSyncItem appapi, T? data, CancellationToken ct = default) where T : IKubernetes;
}
