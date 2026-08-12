using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.Apis;
using PocketIdSync.Models;
using PocketIdSync.ModelSpecs;

namespace PocketIdSync.Repositories;

sealed class OidcClientRepository
{
    private readonly AppApiResolver AppApiResolver = new();
    private bool HasAppApiResolverData = false;
    private readonly UserGroupResolver UserGroupResolver = new();
    private bool HasUserGroupResolverData = false;

    public async Task<ApiResult<OidcClientCompleteDto>> GetAsync(PocketIdClient pocketId, string clientId, CancellationToken ct)
    {
        var client = await pocketId.OidcClients.Id(clientId).GetAsync(ct);
        if (!client.IsSuccessful || client.Data is null)
        {
            return client;
        }
        // add usergroup information

        // add api client access information
        var clientAccess = await pocketId.OidcClients.Id(clientId).GetClientAccess(ct);
        if (!clientAccess.IsSuccessful)
        {
            return client; // TODO: wrong return, must be error ...
        }
        if (clientAccess.Data is not null && (clientAccess.Data.ClientPermissionIds.Length != 0 || clientAccess.Data.UserDelegatedPermissionIds.Length != 0))
        {
            if (!HasAppApiResolverData)
            {
                var appApiResponse = await AppApiResolver.Initialize(pocketId, ct);
                if (!appApiResponse.IsSuccessful)
                {
                    return client; // TODO: wrong return, must be error
                }
                HasAppApiResolverData = true;
            }
            client.Data.ClientPermissions = AppApiResolver.ToPermissions(clientAccess.Data.ClientPermissionIds);
            client.Data.UserDelegatedPermissions = AppApiResolver.ToPermissions(clientAccess.Data.UserDelegatedPermissionIds);
        }

        return client;
    }

    public async Task<ApiResult<OidcClientCompleteDto>?> AmendAsync(PocketIdClient pocketId, string? clientId, OidcClientSpec clientData, CancellationToken ct)
    {
        var oidcClient = clientData.FromKind();
        if (clientData.ClientPermissions is not null || clientData.UserDelegatedPermissions is not null)
        {
            if (!HasAppApiResolverData)
            {
                var appApiResponse = await AppApiResolver.Initialize(pocketId, ct);
                if (!appApiResponse.IsSuccessful)
                {
                    return null; // TODO: wrong return, must be error
                }
                HasAppApiResolverData = true;
            }
            oidcClient.ClientPermissions = AppApiResolver.ToPermissions(clientData.ClientPermissions);
            oidcClient.UserDelegatedPermissions = AppApiResolver.ToPermissions(clientData.UserDelegatedPermissions);
        }
        if (oidcClient.IsGroupRestricted == true)
        {
            if (!HasUserGroupResolverData)
            {
                var userGroupResponse = await UserGroupResolver.Initialize(pocketId, ct);
                if (!userGroupResponse.IsSuccessful)
                {
                    return null; // TODO: wrong return, must be error
                }
                HasUserGroupResolverData = true;
            }
            oidcClient.AllowedUserGroups = UserGroupResolver.ToGroups(clientData.AllowedGroups);
        }

        var baseResponse = clientId is null ?
            await pocketId.OidcClients.PostAsync(oidcClient.ToCreateRequest(), ct) :
            await pocketId.OidcClients.Id(oidcClient.Id!).PutAsync(oidcClient.ToUpdateRequest(), ct);
        if (!baseResponse.IsSuccessful)
        {
            return baseResponse;
        }
        // TODO: ...
        return baseResponse;
    }
}
