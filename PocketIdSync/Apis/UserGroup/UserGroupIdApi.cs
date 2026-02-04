using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.Models;
using RestSharp;

namespace PocketIdSync.Apis.UserGroup;

sealed class UserGroupsIdApi(PocketIdClient PocketId, string Id)
{
    public async Task<ApiResult<UserGroupDto>> GetAsync(CancellationToken ct)
    {
        var request = new RestRequest("/user-groups/{id}").AddUrlSegment("id", Id);
        var response = await PocketId.Api.ExecuteGetAsync<UserGroupDto>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data);
        }
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return response.Ok<UserGroupDto>();
        }
        return response.Nok<UserGroupDto>();
    }

    public async Task<ApiResult<UserGroupDto>> PutAsync(UserGroupDto data, CancellationToken ct)
    {
        var request = new RestRequest("/user-groups/{id}").AddUrlSegment("id", Id).AddBody(data);
        var response = await PocketId.Api.ExecutePutAsync<UserGroupDto>(request, ct);
        if (response.IsSuccessful)
        {
            return await GetAsync(ct);
        }
        return response.Nok<UserGroupDto>(response.Content);
    }

    public async Task<ApiResult<int>> DeleteAsync(CancellationToken ct)
    {
        var request = new RestRequest("/user-groups/{id}").AddUrlSegment("id", Id);
        var response = await PocketId.Api.ExecuteDeleteAsync(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(0);
        }
        return response.Nok<int>(response.Content);
    }
}
