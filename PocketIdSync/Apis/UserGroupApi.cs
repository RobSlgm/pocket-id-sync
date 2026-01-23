using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.Models;
using RestSharp;

namespace PocketIdSync.Apis;

sealed class UserGroupsApi(PocketIdClient PocketId)
{
    public UserGroupsIdApi Id(string id) => new(PocketId, id);

    public async Task<ApiResult<UserGroupMinimalDto[]>> ListAsync(CancellationToken ct)
    {
        var request = new RestRequest("/user-groups").AddQueryParameter("sort[column]", "name");
        var response = await PocketId.Api.ExecuteGetAsync<Paginated<UserGroupMinimalDto>>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data?.Data);
        }
        return response.Nok<UserGroupMinimalDto[]>();
    }

    public async Task<ApiResult<UserGroupDto>> PostAsync(UserGroupDto data, CancellationToken ct)
    {
        var request = new RestRequest("/user-groups").AddBody(data);
        var response = await PocketId.Api.ExecutePostAsync<UserGroupDto>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data);
        }
        return response.Nok<UserGroupDto>(response.Content);
    }
}
