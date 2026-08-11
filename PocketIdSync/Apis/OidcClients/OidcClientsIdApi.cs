using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.Models;
using RestSharp;

namespace PocketIdSync.Apis.OidcClients;

sealed class OidcClientsIdApi(PocketIdClient PocketId, string Id)
{
    public OidcClientsLogoApi Logo(LogoThemeMode theme) => new(PocketId, Id, theme);

    public async Task<ApiResult<OidcClientCompleteDto>> GetAsync(CancellationToken ct)
    {
        var request = new RestRequest("/oidc/clients/{id}").AddUrlSegment("id", Id);
        var response = await PocketId.Api.ExecuteGetAsync<OidcClientCompleteDto>(request, ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return response.Ok<OidcClientCompleteDto>();
        }
        if (!response.IsSuccessful || response.Data is null)
        {
            return response.Nok<OidcClientCompleteDto>();
        }
        var clientAccessRequest = new RestRequest("/api-access/{id}").AddUrlSegment("id", Id);
        var clientAccessResponse = await PocketId.Api.ExecuteGetAsync<ClientApiAccessDto>(clientAccessRequest, ct);
        if (!clientAccessResponse.IsSuccessful)
        {
            return clientAccessResponse.Nok<OidcClientCompleteDto>();
        }
        if (clientAccessResponse.Data is not null)
        {
            response.Data.UserDelegatedPermissionIds = clientAccessResponse.Data.UserDelegatedPermissionIds;
            response.Data.ClientPermissionIds = clientAccessResponse.Data.ClientPermissionIds;
        }
        return response.Ok(response.Data);
    }

    public async Task<ApiResult<OidcClientCompleteDto>> PutAsync(OidcClientUpdateDto data, CancellationToken ct)
    {
        var request = new RestRequest("/oidc/clients/{id}").AddUrlSegment("id", Id).AddBody(data);
        var response = await PocketId.Api.ExecutePutAsync<OidcClientCompleteDto>(request, ct);
        if (response.IsSuccessful)
        {
            return await GetAsync(ct);
        }
        return response.Nok<OidcClientCompleteDto>(response.Content);
    }

    public async Task<ApiResult<int>> DeleteAsync(CancellationToken ct)
    {
        var request = new RestRequest("/oidc/clients/{id}").AddUrlSegment("id", Id);
        var response = await PocketId.Api.ExecuteDeleteAsync(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(0);
        }
        return response.Nok<int>(response.Content);
    }

    public async Task<ApiResult<OidcClientCompleteDto>> PutAllowedUserGroupsAsync(UserGroupMinimalDto[] data, CancellationToken ct)
    {
        var body = new UpdateAllowedUserGroupsDto { UserGroupIds = data.Select(g => g.Id).ToArray()! };
        var request = new RestRequest("/oidc/clients/{id}/allowed-user-groups").AddUrlSegment("id", Id).AddBody(body);
        var response = await PocketId.Api.ExecutePutAsync<OidcClientCompleteDto>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data);
        }
        return response.Nok<OidcClientCompleteDto>(response.Content);
    }

    public async Task<ApiResult<SecretDto>> SetSecretAsync(CancellationToken ct)
    {
        // var body = new UpdateAllowedUserGroupsDto { UserGroupIds = data.Select(g => g.Id).ToArray()! };
        var request = new RestRequest("/oidc/clients/{id}/secret").AddUrlSegment("id", Id);
        var response = await PocketId.Api.ExecutePostAsync<SecretDto>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data);
        }
        return response.Nok<SecretDto>(response.Content);
    }

    public async Task<ApiResult<ClientApiAccessDto>> GetClientAccess(CancellationToken ct)
    {
        var request = new RestRequest("/api-access/{id}").AddUrlSegment("id", Id);
        var response = await PocketId.Api.ExecuteGetAsync<ClientApiAccessDto>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data);
        }
        // if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        // {
        //     return response.Ok<ClientApiAccessDto>();
        // }
        return response.Nok<ClientApiAccessDto>();
    }

    public async Task<ApiResult<ClientApiAccessDto>> UpdateClientAccess(ClientApiAccessUpdateDto body, CancellationToken ct)
    {
        var request = new RestRequest("/api-access/{id}").AddUrlSegment("id", Id).AddBody(body);
        var response = await PocketId.Api.ExecutePutAsync<ClientApiAccessDto>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data);
        }
        return response.Nok<ClientApiAccessDto>();
    }
}
