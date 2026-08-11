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

    public async Task<ApiResult<ApiResponseDto>> PutAsync(ApiUpdateDto data, CancellationToken ct)
    {
        var request = new RestRequest("/apis/{id}").AddUrlSegment("id", Id).AddBody(data);
        var response = await PocketId.Api.ExecutePutAsync<ApiResponseDto>(request, ct);
        if (response.IsSuccessful)
        {
            return await GetAsync(ct);
        }
        return response.Nok<ApiResponseDto>(response.Content);
    }

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

    public async Task<ApiResult<ApiResponseDto>> UpdatePermissions(ApiPermissionInputDto[] data, CancellationToken ct)
    {
        var body = new ApiPermissionsUpdateDto { Permissions = [.. data], };
        var request = new RestRequest("/apis/{id}/permissions").AddUrlSegment("id", Id).AddBody(body);
        var response = await PocketId.Api.ExecutePutAsync<ApiResponseDto>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data);
        }
        return response.Nok<ApiResponseDto>(response.Content);
    }
}
