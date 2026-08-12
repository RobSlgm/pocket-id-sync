
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.Apis;
using PocketIdSync.Models;

namespace PocketIdSync.Repositories;

sealed class OidcClientRepository
{
    private readonly AppApiResolver Resolver = new();
    private bool IsInitialized = false;

    public async Task<ApiResult<OidcClientCompleteDto>> GetAsync(PocketIdClient pocketId, string clientId, CancellationToken ct)
    {
        var client = await pocketId.OidcClients.Id(clientId).GetAsync(ct);
        if (!client.IsSuccessful || client.Data is null)
        {
            return client;
        }
        var clientAccess = await pocketId.OidcClients.Id(clientId).GetClientAccess(ct);
        if (!clientAccess.IsSuccessful)
        {
            return client; // TODO: wrong return, must be error ...
        }
        if (clientAccess.Data is null || (clientAccess.Data.ClientPermissionIds.Length == 0 && clientAccess.Data.UserDelegatedPermissionIds.Length == 0))
        {
            return client;
        }
        if (!IsInitialized)
        {
            var appApiResponse = await Resolver.Initialize(pocketId, ct);
            if (!appApiResponse.IsSuccessful)
            {
                return client; // TODO: wrong return, must be error
            }
            IsInitialized = true;
        }
        client.Data.ClientPermissions = ToPermissions(clientAccess.Data.ClientPermissionIds);
        client.Data.UserDelegatedPermissions = ToPermissions(clientAccess.Data.UserDelegatedPermissionIds);

        return client;
    }

    private ApiPermissionMinimalDto[]? ToPermissions(string[]? permissionIds)
    {
        if (permissionIds is null || permissionIds.Length == 0)
        {
            return null;
        }
        var permissions = new List<ApiPermissionMinimalDto>();
        foreach (var pid in permissionIds)
        {
            var permission = Resolver.Find(pid);
            if (permission is not null)
            {
                permissions.Add(permission);
            }
        }
        return permissions.Count > 0 ? [.. permissions] : null;
    }
}
