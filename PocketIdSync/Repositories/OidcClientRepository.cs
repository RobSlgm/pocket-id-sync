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
        // add api client access information
        var clientAccess = await pocketId.OidcClients.Id(clientId).GetClientAccess(ct);
        if (!clientAccess.IsSuccessful)
        {
            return clientAccess.NokAs<OidcClientCompleteDto, ClientApiAccessDto>();
        }
        if (clientAccess.Data is not null && (clientAccess.Data.ClientPermissionIds.Length != 0 || clientAccess.Data.UserDelegatedPermissionIds.Length != 0))
        {
            var appApiResponse = await EnsureAppApiDataAsync(pocketId, ct);
            if (!appApiResponse.IsSuccessful)
            {
                return appApiResponse.NokAs<OidcClientCompleteDto, ApiResponseDto[]>();
            }
            client.Data.ClientPermissions = AppApiResolver.ToPermissions(clientAccess.Data.ClientPermissionIds);
            client.Data.UserDelegatedPermissions = AppApiResolver.ToPermissions(clientAccess.Data.UserDelegatedPermissionIds);
        }

        return client;
    }

    public async Task<(int ExitCode, OidcClientCompleteDto? Client, string? ErrorMessage)> FromKindAsync(PocketIdClient pocketId, OidcClientSpec clientData, CancellationToken ct)
    {
        var oidcClient = clientData.FromKind();
        if (clientData.ClientPermissions is not null || clientData.UserDelegatedPermissions is not null)
        {
            var appApiResponse = await EnsureAppApiDataAsync(pocketId, ct);
            if (!appApiResponse.IsSuccessful)
            {
                return (ExitCode.FatalError, null, "Failed to load application api data");
            }
            oidcClient.ClientPermissions = AppApiResolver.ToPermissions(clientData.ClientPermissions);
            oidcClient.UserDelegatedPermissions = AppApiResolver.ToPermissions(clientData.UserDelegatedPermissions);
        }
        if (oidcClient.IsGroupRestricted == true)
        {
            var userGroupResponse = await EnsureUserGroupDataAsync(pocketId, ct);
            if (!userGroupResponse.IsSuccessful)
            {
                return (ExitCode.FatalError, null, "Failed to load user group data");
            }
            oidcClient.AllowedUserGroups = UserGroupResolver.ToGroups(clientData.AllowedGroups);
        }
        return (ExitCode.Success, oidcClient, null);
    }


    public async Task<ApiResult<OidcClientCompleteDto>?> AmendAsync(PocketIdClient pocketId, string? clientId, OidcClientCompleteDto oidcClient, CancellationToken ct)
    {
        var baseResponse = clientId is null ?
            await pocketId.OidcClients.PostAsync(oidcClient.ToCreateRequest(), ct) :
            await pocketId.OidcClients.Id(oidcClient.Id!).PutAsync(oidcClient.ToUpdateRequest(), ct);
        if (!baseResponse.IsSuccessful || baseResponse.Data is null)
        {
            return baseResponse;
        }
        // TODO: Check if groups in local and remote are changed
        var groups = await pocketId.OidcClients.Id(baseResponse.Data.Id!).PutAllowedUserGroupsAsync(oidcClient.AllowedUserGroups, ct);
        if (!groups.IsSuccessful)
        {
            return groups;
        }
        // TODO: Check if api permissions in local and remote are changed
        var permissions = await pocketId.OidcClients.Id(baseResponse.Data.Id!).UpdateClientAccess(oidcClient.ToClientApiAccessUpdateRequest(), ct);
        if (!permissions.IsSuccessful)
        {
            return permissions.NokAs<OidcClientCompleteDto, ClientApiAccessDto>();
        }
        return baseResponse;
    }

    private async Task<ApiResult<ApiResponseDto[]>> EnsureAppApiDataAsync(PocketIdClient pocketId, CancellationToken ct)
    {
        if (HasAppApiResolverData)
        {
            return new ApiResult<ApiResponseDto[]>(IsSuccessful: true);
        }
        var appApiResponse = await AppApiResolver.Initialize(pocketId, ct);
        if (appApiResponse.IsSuccessful)
        {
            HasAppApiResolverData = true;
        }
        return appApiResponse;
    }

    private async Task<ApiResult<UserGroupMinimalDto[]>> EnsureUserGroupDataAsync(PocketIdClient pocketId, CancellationToken ct)
    {
        if (HasUserGroupResolverData)
        {
            return new ApiResult<UserGroupMinimalDto[]>(IsSuccessful: true);
        }
        var userGroupResponse = await UserGroupResolver.Initialize(pocketId, ct);
        if (userGroupResponse.IsSuccessful)
        {
            HasUserGroupResolverData = true;
        }
        return userGroupResponse;
    }
}
