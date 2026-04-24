using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.Models;
using RestSharp;

namespace PocketIdSync.Apis;

sealed class VersionApi(PocketIdClient PocketId)
{
    public async Task<ApiResult<VersionInfoDto>> GetAsync(CancellationToken ct)
    {
        var requestCurrent = new RestRequest("/version/current");
        var responseCurrent = await PocketId.Api.ExecuteGetAsync<VersionInfoDto>(requestCurrent, ct);
        if (!responseCurrent.IsSuccessful)
        {
            responseCurrent.Nok<VersionInfoDto>();
        }
        var result = responseCurrent.Ok(responseCurrent.Data);
        var requestLatest = new RestRequest("/version/latest");
        var responseLatest = await PocketId.Api.ExecuteGetAsync<VersionInfoDto>(requestLatest, ct);
        if (!responseLatest.IsSuccessful)
        {
            responseCurrent.Nok<VersionInfoDto>();
        }
        result.Data!.LatestVersion = responseLatest.Data?.LatestVersion;
        return result;
    }
}
