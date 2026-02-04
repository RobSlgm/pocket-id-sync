using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.Models;
using RestSharp;

namespace PocketIdSync.Apis.ApiKeys;

sealed class ApiKeysApi(PocketIdClient pocketId)
{
    public ApiKeysIdApi Id(string id) => new(pocketId, id);

    public async Task<ApiResult<ApiKeyDto[]>> ListAsync(CancellationToken ct)
    {
        var request = new RestRequest("/api-keys").AddQueryParameter("sort[column]", "name");
        var response = await pocketId.Api.ExecuteGetAsync<Paginated<ApiKeyDto>>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data?.Data);
        }
        return response.Nok<ApiKeyDto[]>();
    }

    public async Task<ApiResult<ApiKeyResponseDto>> PostAsync(ApiKeyCreateDto data, CancellationToken ct)
    {
        var request = new RestRequest("/api-keys").AddBody(data);
        var response = await pocketId.Api.ExecutePostAsync<ApiKeyResponseDto>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data);
        }
        return response.Nok<ApiKeyResponseDto>(response.Content);
    }
}
