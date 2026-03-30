using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.Models;
using RestSharp;

namespace PocketIdSync.Apis.ApplicationConfiguration;

sealed class ApplicationConfigurationApi(PocketIdClient pocketId)
{

    public async Task<ApiResult<AppConfigVariableDto[]>> ListAllAsync(CancellationToken ct)
    {
        var request = new RestRequest("/application-configuration/all");
        var response = await pocketId.Api.ExecuteGetAsync<AppConfigVariableDto[]>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data);
        }
        return response.Nok<AppConfigVariableDto[]>();
    }

    public async Task<ApiResult<PublicAppConfigVariableDto[]>> ListAsync(CancellationToken ct)
    {
        var request = new RestRequest("/application-configuration");
        var response = await pocketId.Api.ExecuteGetAsync<PublicAppConfigVariableDto[]>(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.Data);
        }
        return response.Nok<PublicAppConfigVariableDto[]>();
    }
}
