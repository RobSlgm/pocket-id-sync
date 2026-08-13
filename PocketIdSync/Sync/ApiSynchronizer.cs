using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.Apis;
using PocketIdSync.Models;
using PocketIdSync.ModelSpecs;
using PocketIdSync.Utils;

namespace PocketIdSync.Sync;

sealed class ApiSynchronizer : ISynchronizer<ApiSyncItem>
{
    public List<ApiSyncItem> Items { get; private set; } = [];
    private readonly IConfigStoreApi Configuration;

    public ApiSynchronizer(IConfigStoreApi configuration)
    {
        Configuration = configuration;
    }

    public async Task<(int ExitCode, ApiSyncItem? Client, string? ErrorMessage)> CombineAsync(PocketIdClient pocketId, SynchronizationTarget direction, SyncItemSelector? selector, CancellationToken ct)
    {
        return direction == SynchronizationTarget.PocketID ?
          await MergeFromPocketIdAsync(pocketId, ct) :
          await MergeFromConfigurationAsync(pocketId, selector, ct);
    }


    private async Task<(int ExitCode, ApiSyncItem? Client, string? ErrorMessage)> MergeFromPocketIdAsync(PocketIdClient pocketId, CancellationToken ct)
    {
        var allShortResponse = await pocketId.Apis.ListAsync(ct: ct);
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

    private async Task<(int ExitCode, ApiSyncItem? Client, string? ErrorMessage)> MergeFromConfigurationAsync(PocketIdClient pocketId, SyncItemSelector? selector, CancellationToken ct)
    {
        var allShortResponse = await pocketId.Apis.ListAsync(ct: ct);
        if (!allShortResponse.IsSuccessful)
        {
            return (ExitCode.FatalError, default, default);
        }
        foreach (var api in allShortResponse.Data ?? [])
        {
            if (string.IsNullOrEmpty(api.Id)) continue;
            var sync = new ApiSyncItem
            {
                Id = StringNameConverter.ToSafeName(api.Resource),
                Name = api.Name,
            };
            if (!sync.IsMatch(selector))
            {
                continue;
            }
            var apiResponse = await pocketId.Apis.Id(api.Id).GetAsync(ct);
            if (!apiResponse.IsSuccessful)
            {
                return (ExitCode.FatalError, sync, $"Merge failed {apiResponse.Status}");
            }
            sync.Remote = apiResponse.Data;
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

    private static async Task<ApiResult<ApiResponseDto>> UpdateAsync(PocketIdClient pocketId, string? id, ApiResponseDto api, CancellationToken ct)
    {
        var header = !string.IsNullOrEmpty(id) ? await pocketId.Apis.Id(id).PutAsync(api.ToUpdateRequest(), ct) : await pocketId.Apis.PostAsync(api.ToCreateRequest(), ct);
        if (!header.IsSuccessful || header.Data is null)
        {
            return header;
        }
        var apiId = header.Data.Id!;
        var permissions = await pocketId.Apis.Id(apiId).UpdatePermissions(api.Permissions.ToUpdateRequest(), ct);
        // if (!permissions.IsSuccessful)
        // {
        //     return permissions;
        // }
        return permissions;
    }

    private async Task<int> SynchronizeToPocketIdAsync(PocketIdClient pocketId, CancellationToken ct)
    {
        var exitCode = ExitCode.Success;
        foreach (var sync in Items.Where(c => (c.IsRemoteEqualLocal == false) && c.HasError == false))
        {
            if (sync.Local?.Spec is null) continue;
            var api = sync.Local.Spec.FromKind(sync.Remote);
            if (sync.IsRemoteEqualLocal == false)
            {
                var changed = await UpdateAsync(pocketId, sync.Remote?.Id, api, ct);
                if (!changed.IsSuccessful)
                {
                    sync.SetError(changed.ErrorMessage);
                    exitCode = ExitCode.GeneralError;
                    continue;
                }
                sync.RemoteMerged = changed.Data;
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
