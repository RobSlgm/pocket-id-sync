using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.Models;
using RestSharp;

namespace PocketIdSync.Apis.OidcClients;

sealed class OidcClientsIdApi(PocketIdClient PocketId, string Id)
{
    public OidcClientsLogoApi Logo(LogoThemeMode theme) => new(PocketId, Id, theme);

    public async Task<ApiResult<OidcClientWithAllowedGroupsDto>> GetAsync(CancellationToken ct)
    {
        var request = new RestRequest("/oidc/clients/{id}").AddUrlSegment("id", Id);
        var response = await PocketId.Api.ExecuteGetAsync<OidcClientWithAllowedGroupsDto>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data);
        }
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return response.Ok<OidcClientWithAllowedGroupsDto>();
        }
        return response.Nok<OidcClientWithAllowedGroupsDto>();
    }

    public async Task<ApiResult<OidcClientWithAllowedGroupsDto>> PutAsync(OidcClientUpdateDto data, CancellationToken ct)
    {
        var request = new RestRequest("/oidc/clients/{id}").AddUrlSegment("id", Id).AddBody(data);
        var response = await PocketId.Api.ExecutePutAsync<OidcClientWithAllowedGroupsDto>(request, ct);
        if (response.IsSuccessful)
        {
            return await GetAsync(ct);
        }
        return response.Nok<OidcClientWithAllowedGroupsDto>(response.Content);
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

    public async Task<ApiResult<OidcClientWithAllowedGroupsDto>> PutAllowedUserGroupsAsync(UserGroupMinimalDto[] data, CancellationToken ct)
    {
        var body = new UpdateAllowedUserGroupsDto { UserGroupIds = data.Select(g => g.Id).ToArray()! };
        var request = new RestRequest("/oidc/clients/{id}/allowed-user-groups").AddUrlSegment("id", Id).AddBody(body);
        var response = await PocketId.Api.ExecutePutAsync<OidcClientWithAllowedGroupsDto>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data);
        }
        return response.Nok<OidcClientWithAllowedGroupsDto>(response.Content);
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
}
