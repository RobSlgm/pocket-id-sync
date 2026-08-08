using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.Models;
using RestSharp;

namespace PocketIdSync.Apis.AppApis;

sealed class AppApisIdApi(PocketIdClient PocketId, string Id)
{
    public async Task<ApiResult<ApiResponseDto>> GetAsync(CancellationToken ct)
    {
        var request = new RestRequest("/apis/{id}").AddUrlSegment("id", Id);
        var response = await PocketId.Api.ExecuteGetAsync<ApiResponseDto>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data);
        }
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return response.Ok<ApiResponseDto>();
        }
        return response.Nok<ApiResponseDto>();
    }

    // public async Task<ApiResult<ApiResponseDto>> PutAsync(OidcClientUpdateDto data, CancellationToken ct)
    // {
    //     var request = new RestRequest("/oidc/clients/{id}").AddUrlSegment("id", Id).AddBody(data);
    //     var response = await PocketId.Api.ExecutePutAsync<ApiResponseDto>(request, ct);
    //     if (response.IsSuccessful)
    //     {
    //         return await GetAsync(ct);
    //     }
    //     return response.Nok<ApiResponseDto>(response.Content);
    // }

    public async Task<ApiResult<int>> DeleteAsync(CancellationToken ct)
    {
        var request = new RestRequest("/apis/{id}").AddUrlSegment("id", Id);
        var response = await PocketId.Api.ExecuteDeleteAsync(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(0);
        }
        return response.Nok<int>(response.Content);
    }

    // public async Task<ApiResult<ApiResponseDto>> PutAllowedUserGroupsAsync(UserGroupMinimalDto[] data, CancellationToken ct)
    // {
    //     var body = new UpdateAllowedUserGroupsDto { UserGroupIds = data.Select(g => g.Id).ToArray()! };
    //     var request = new RestRequest("/oidc/clients/{id}/allowed-user-groups").AddUrlSegment("id", Id).AddBody(body);
    //     var response = await PocketId.Api.ExecutePutAsync<ApiResponseDto>(request, ct);
    //     if (response.IsSuccessful)
    //     {
    //         return response.Ok(response.Data);
    //     }
    //     return response.Nok<ApiResponseDto>(response.Content);
    // }

    // public async Task<ApiResult<SecretDto>> SetSecretAsync(CancellationToken ct)
    // {
    //     // var body = new UpdateAllowedUserGroupsDto { UserGroupIds = data.Select(g => g.Id).ToArray()! };
    //     var request = new RestRequest("/oidc/clients/{id}/secret").AddUrlSegment("id", Id);
    //     var response = await PocketId.Api.ExecutePostAsync<SecretDto>(request, ct);
    //     if (response.IsSuccessful)
    //     {
    //         return response.Ok(response.Data);
    //     }
    //     return response.Nok<SecretDto>(response.Content);
    // }
}
