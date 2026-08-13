using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.Models;
using RestSharp;

namespace PocketIdSync.Apis.Apis;

sealed class ApisApi(PocketIdClient pocketId)
{
    public ApisIdApi Id(string id) => new(pocketId, id);

    public async Task<ApiResult<ApiResponseDto[]>> ListAsync(string? searchQuery = null, CancellationToken ct = default)
    {
        var request = new RestRequest("/apis").AddQueryParameter("sort[column]", "name");
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            request.AddQueryParameter("search", searchQuery);
        }
        var response = await pocketId.Api.ExecuteGetAsync<Paginated<ApiResponseDto>>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data?.Data);
        }
        return response.Nok<ApiResponseDto[]>();
    }

    public async Task<ApiResult<ApiResponseDto>> PostAsync(ApiCreateDto data, CancellationToken ct)
    {
        var request = new RestRequest("/apis").AddBody(data);
        var response = await pocketId.Api.ExecutePostAsync<ApiResponseDto>(request, ct);
        if (response.IsSuccessful && response.Data is not null)
        {
            return await pocketId.Apis.Id(response.Data.Id!).GetAsync(ct);
        }
        return response.Nok<ApiResponseDto>(response.Content);
    }
}
