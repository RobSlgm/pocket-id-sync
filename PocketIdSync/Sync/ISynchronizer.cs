using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.Apis;

namespace PocketIdSync.Sync;

internal interface ISynchronizer<T>
{
    List<T> Items { get; }

    Task<int> LoadConfiguration(SyncItemSelector? selector, CancellationToken ct);
    Task<(int ExitCode, T? Client, string? ErrorMessage)> CombineAsync(PocketIdClient pocketId, SynchronizationTarget direction, SyncItemSelector? selector, CancellationToken ct);
    Task<int> SynchronizeAsync(PocketIdClient pocketId, SynchronizationTarget direction, CancellationToken ct);
}
