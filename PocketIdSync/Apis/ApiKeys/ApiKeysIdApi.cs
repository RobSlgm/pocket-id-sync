using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.Models;
using RestSharp;

namespace PocketIdSync.Apis.ApiKeys;

sealed class ApiKeysIdApi(PocketIdClient PocketId, string Id)
{
    public async Task<ApiResult<int>> RevokeAsync(CancellationToken ct)
    {
        var request = new RestRequest("/api-keys/{id}").AddUrlSegment("id", Id);
        var response = await PocketId.Api.ExecuteDeleteAsync(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(0);
        }
        return response.Nok<int>(response.Content);
    }

    public async Task<ApiResult<ApiKeyResponseDto>> RenewAsync(ApiKeyRenewDto data, CancellationToken ct)
    {
        var request = new RestRequest("/api-keys/{id}/renew").AddUrlSegment("id", Id).AddBody(data);
        var response = await PocketId.Api.ExecutePostAsync<ApiKeyResponseDto>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data);
        }
        return response.Nok<ApiKeyResponseDto>(response.Content);
    }
}
