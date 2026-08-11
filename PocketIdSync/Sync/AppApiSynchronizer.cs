using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.Apis;
using PocketIdSync.ModelSpecs;
using PocketIdSync.Utils;

namespace PocketIdSync.Sync;

sealed class AppApiSynchronizer : ISynchronizer<AppApiSyncItem>
{
    public List<AppApiSyncItem> Items { get; private set; } = [];
    private readonly IConfigStoreAppApi Configuration;

    public AppApiSynchronizer(IConfigStoreAppApi configuration)
    {
        Configuration = configuration;
    }

    public async Task<(int ExitCode, AppApiSyncItem? Client, string? ErrorMessage)> CombineAsync(PocketIdClient pocketId, SynchronizationTarget direction, SyncItemSelector? selector, CancellationToken ct)
    {
        return direction == SynchronizationTarget.PocketID ?
          await MergeFromPocketIdAsync(pocketId, ct) :
          await MergeFromConfigurationAsync(pocketId, selector, ct);
    }


    private async Task<(int ExitCode, AppApiSyncItem? Client, string? ErrorMessage)> MergeFromPocketIdAsync(PocketIdClient pocketId, CancellationToken ct)
    {
        var allShortResponse = await pocketId.AppApis.ListAsync(ct: ct);
        if (!allShortResponse.IsSuccessful)
        {
            return (ExitCode.FatalError, default, default);
        }
        foreach (var sync in Items)
        {
            if (sync.Local?.Spec?.Resource is not null)
            {
                var source = allShortResponse.Data?.FirstOrDefault(g => string.Equals(sync.Local.Spec.Resource, g.Resource, StringComparison.OrdinalIgnoreCase));
                if (source is not null)
                {
                    sync.Remote = source;
                    sync.Id = source.Id;
                }
            }
        }
        return (ExitCode.Success, default, default);
    }

    private async Task<(int ExitCode, AppApiSyncItem? Client, string? ErrorMessage)> MergeFromConfigurationAsync(PocketIdClient pocketId, SyncItemSelector? selector, CancellationToken ct)
    {
        var allShortResponse = await pocketId.AppApis.ListAsync(ct: ct);
        if (!allShortResponse.IsSuccessful)
        {
            return (ExitCode.FatalError, default, default);
        }
        foreach (var appApi in allShortResponse.Data ?? [])
        {
            if (string.IsNullOrEmpty(appApi.Id)) continue;
            var sync = new AppApiSyncItem
            {
                Id = StringNameConverter.ToSafeName(appApi.Resource),
                Name = appApi.Name,
            };
            if (!sync.IsMatch(selector))
            {
                continue;
            }
            var appApiResponse = await pocketId.AppApis.Id(appApi.Id).GetAsync(ct);
            if (!appApiResponse.IsSuccessful)
            {
                return (ExitCode.FatalError, sync, $"Merge failed {appApiResponse.Status}");
            }
            sync.Remote = appApiResponse.Data;
            Items.Add(sync);
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
        foreach (var sync in Items.Where(c => (c.IsRemoteEqualLocal == false) && c.HasError == false))
        {
            if (sync.Local?.Spec is null) continue;
            var appApi = sync.Local.Spec.FromKind(sync.Remote);
            if (sync.IsRemoteEqualLocal == false)
            {
                if (sync.Remote is not null)
                {
                    var xxx = appApi.ToUpdateRequest();
                    var update = await pocketId.AppApis.Id(sync.Remote.Id!).PutAsync(appApi.ToUpdateRequest(), ct);
                    if (!update.IsSuccessful)
                    {
                        sync.SetError(update.ErrorMessage);
                        exitCode = ExitCode.GeneralError;
                        continue;
                    }
                    sync.RemoteMerged = update.Data;
                }
                else
                {
                    var create = await pocketId.AppApis.PostAsync(appApi.ToCreateRequest(), ct);
                    if (!create.IsSuccessful)
                    {
                        sync.SetError(create.ErrorMessage);
                        exitCode = ExitCode.GeneralError;
                        continue;
                    }
                    sync.RemoteMerged = create.Data;
                }
            }
        }
        return exitCode;
    }

    private async Task<int> SynchronizeToConfigurationAsync(PocketIdClient pocketId, CancellationToken ct)
    {
        foreach (var sync in Items.Where(c => c.Remote is not null && c.HasError == false))
        {
            var syncResponse = await Configuration.SynchronizeAsync(sync, ct);
            if (syncResponse != ExitCode.Success)
            {
                sync.SetError("Sync");
                continue;
            }
            if (sync.HasError == false && sync.LocalMerged is not null && sync.IsLocalDirty == true)
            {
                var store = await Configuration.WriteAsync(sync, sync.LocalMerged, ct);
                if (store != ExitCode.Success)
                {
                    sync.SetError("Local store write");
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
        foreach (var sync in Items)
        {
            if (sync.Local is null || sync.Local.Spec is null)
            {
                sync.SetError($"Local specification missing or invalid");
                continue;
            }
            var local = sync.Local.Spec;
            if (local is null) continue;
        }
        int removeFailed = Items.RemoveAll(c => c.Local is null || c.Local.Spec is null || c.HasError == true);
        return removeFailed == 0 ? ExitCode.Success : ExitCode.GeneralError;
    }
}
