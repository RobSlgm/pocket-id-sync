using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.Models;
using RestSharp;

namespace PocketIdSync.Apis.OidcClientApis;

sealed class OidcClientApisApi(PocketIdClient pocketId)
{
    public OidcClientApisIdApi Id(string id) => new(pocketId, id);

    public async Task<ApiResult<OidcClientWithAllowedGroupsCountDto[]>> ListAsync(CancellationToken ct)
    {
        var request = new RestRequest("/api/apis").AddQueryParameter("sort[column]", "name");
        var response = await pocketId.Api.ExecuteGetAsync<Paginated<OidcClientWithAllowedGroupsCountDto>>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data?.Data);
        }
        return response.Nok<OidcClientWithAllowedGroupsCountDto[]>();
    }

    // public async Task<ApiResult<OidcClientWithAllowedGroupsDto>> PostAsync(OidcClientCreateDto data, CancellationToken ct)
    // {
    //     var request = new RestRequest("/oidc/clients").AddBody(data);
    //     var response = await pocketId.Api.ExecutePostAsync<OidcClientWithAllowedGroupsDto>(request, ct);
    //     if (response.IsSuccessful)
    //     {
    //         return await pocketId.OidcClients.Id(data.Id!).GetAsync(ct);
    //     }
    //     return response.Nok<OidcClientWithAllowedGroupsDto>(response.Content);
    // }
}
