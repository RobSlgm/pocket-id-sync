using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.Apis;
using PocketIdSync.Models;
using PocketIdSync.ModelSpecs;

namespace PocketIdSync.Repositories;

sealed class AppApiResolver
{
    public List<ApiResponseDto> AppApis = [];

    public async Task<ApiResult<ApiResponseDto[]>> Initialize(PocketIdClient pocketId, CancellationToken ct)
    {
        var response = await pocketId.AppApis.ListAsync(ct: ct);
        if (!response.IsSuccessful)
        {
            return response;
        }
        if (response.Data is not null)
        {
            AppApis.Clear();
            AppApis.AddRange(response.Data);
        }
        return response;
    }

    public ApiPermissionMinimalDto? Find(string id)
    {
        foreach (var app in AppApis)
        {
            foreach (var permission in app.Permissions)
            {
                if (string.Equals(permission.Id, id, System.StringComparison.Ordinal))
                {
                    return new ApiPermissionMinimalDto
                    {
                        Key = permission.Key!,
                        Resource = app.Resource!,
                        Id = app.Id,
                    };
                }
            }
        }
        return null;
    }

    public ApiPermissionMinimalDto? Find(AppApiPermission permission)
    {
        var app = AppApis.Find(a => string.Equals(a.Resource, permission.Resource, System.StringComparison.Ordinal));
        if (app is not null && app.Resource is not null)
        {
            foreach (var p in app.Permissions)
            {
                if (string.Equals(p.Key, permission.Key, System.StringComparison.Ordinal))
                {
                    return new ApiPermissionMinimalDto
                    {
                        Key = p.Key!,
                        Resource = app.Resource,
                        Id = p.Id,
                    };
                }
            }
        }
        return null;
    }
}

static class AppApiResolverExtensions
{
    extension(AppApiResolver resolver)
    {
        public ApiPermissionMinimalDto[]? ToPermissions(string[]? permissionIds)
        {
            if (permissionIds is null || permissionIds.Length == 0)
            {
                return null;
            }
            var permissions = new List<ApiPermissionMinimalDto>();
            foreach (var pid in permissionIds)
            {
                var permission = resolver.Find(pid);
                if (permission is not null)
                {
                    permissions.Add(permission);
                }
            }
            return permissions.Count > 0 ? [.. permissions] : null;
        }

        public ApiPermissionMinimalDto[]? ToPermissions(AppApiPermission[]? permissionRefs)
        {
            if (permissionRefs is null || permissionRefs.Length == 0)
            {
                return null;
            }
            var permissions = new List<ApiPermissionMinimalDto>();
            foreach (var permRef in permissionRefs)
            {
                var permission = resolver.Find(permRef);
                if (permission is not null)
                {
                    permissions.Add(permission);
                }
            }
            return permissions.Count > 0 ? [.. permissions] : null;
        }
    }
}
