using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.Models;
using RestSharp;

namespace PocketIdSync.Apis;

sealed class VersionApi(PocketIdClient PocketId)
{
    public async Task<ApiResult<string>> GetLatest(CancellationToken ct)
    {
        var request = new RestRequest("/version/latest");
        var response = await PocketId.Api.ExecuteGetAsync<VersionInfoDto>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data?.LatestVersion);
        }
        return response.Nok<string>();
    }
}
