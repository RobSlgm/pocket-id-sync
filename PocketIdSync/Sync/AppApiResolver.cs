
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.Apis;
using PocketIdSync.Models;
using PocketIdSync.ModelSpecs;

namespace PocketIdSync.Sync;

sealed class AppApiResolver
{
    public List<ApiResponseDto> AppApis = [];
    public async Task<ApiResult<ApiResponseDto[]>> Initialize(PocketIdClient pocketId, CancellationToken ct)
    {
        var clients = await pocketId.AppApis.ListAsync(ct: ct);
        if (!clients.IsSuccessful)
        {
            return clients;
        }
        if (clients.Data is not null)
        {
            AppApis.Clear();
            AppApis.AddRange(clients.Data);
        }
        return clients;
    }

    public AppApiPermission? Find(string id)
    {
        foreach (var app in AppApis)
        {
            foreach (var permission in app.Permissions)
            {
                if (string.Equals(permission.Id, id, System.StringComparison.Ordinal))
                {
                    return new AppApiPermission
                    {
                        Key = permission.Key!,
                        Resource = app.Resource!,
                    };
                }
            }
        }
        return null;
    }

    public string? Find(AppApiPermission permission)
    {
        var app = AppApis.Find(a => string.Equals(a.Resource, permission.Resource, System.StringComparison.Ordinal));
        if (app is not null)
        {
            foreach (var p in app.Permissions)
            {
                if (string.Equals(p.Key, permission.Key, System.StringComparison.Ordinal))
                {
                    return p.Id;
                }
            }
        }
        return null;
    }
}
