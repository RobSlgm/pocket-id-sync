using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.Apis;
using PocketIdSync.Models;
using PocketIdSync.ModelSpecs;

namespace PocketIdSync.Repositories;

sealed class OidcClientRepository
{
    private readonly ApiResolver ApiResolver = new();
    private bool HasApiResolverData = false;
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
            var apiResponse = await EnsureApiDataAsync(pocketId, ct);
            if (!apiResponse.IsSuccessful)
            {
                return apiResponse.NokAs<OidcClientCompleteDto, ApiResponseDto[]>();
            }
            client.Data.ClientPermissions = ApiResolver.ToPermissions(clientAccess.Data.ClientPermissionIds);
            client.Data.UserDelegatedPermissions = ApiResolver.ToPermissions(clientAccess.Data.UserDelegatedPermissionIds);
        }

        return client;
    }

    public async Task<(int ExitCode, OidcClientCompleteDto? Client, string? ErrorMessage)> FromKindAsync(PocketIdClient pocketId, OidcClientSpec clientData, CancellationToken ct)
    {
        var oidcClient = clientData.FromKind();
        if (clientData.ClientPermissions is not null || clientData.UserDelegatedPermissions is not null)
        {
            var apiResponse = await EnsureApiDataAsync(pocketId, ct);
            if (!apiResponse.IsSuccessful)
            {
                return (ExitCode.FatalError, null, "Failed to load application api data");
            }

            var (cpCode, cpPermissions, cpErrorMessage) = ApiResolver.TryConvert(clientData.ClientPermissions);
            if (cpCode != ExitCode.Success)
            {
                return (ExitCode.BadRequest, null, $"Application Api client permission not found: {cpErrorMessage}");
            }
            oidcClient.ClientPermissions = cpPermissions;

            var (udCode, udPermissions, udErrorMessage) = ApiResolver.TryConvert(clientData.UserDelegatedPermissions);
            if (udCode != ExitCode.Success)
            {
                return (ExitCode.BadRequest, null, $"Application Api user delegated permission not found: {udErrorMessage}");
            }
            oidcClient.UserDelegatedPermissions = udPermissions;
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


    public static async Task<ApiResult<OidcClientCompleteDto>?> AmendAsync(PocketIdClient pocketId, string? clientId, OidcClientCompleteDto oidcClient, CancellationToken ct)
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

    private async Task<ApiResult<ApiResponseDto[]>> EnsureApiDataAsync(PocketIdClient pocketId, CancellationToken ct)
    {
        if (HasApiResolverData)
        {
            return new ApiResult<ApiResponseDto[]>(IsSuccessful: true);
        }
        var apiResponse = await ApiResolver.Initialize(pocketId, ct);
        if (apiResponse.IsSuccessful)
        {
            HasApiResolverData = true;
        }
        return apiResponse;
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
