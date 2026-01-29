using System.Threading;
using System.Threading.Tasks;
using RestSharp;

namespace PocketIdSync.Apis;

sealed class OidcClientsLogoApi(PocketIdClient PocketId, string Id, LogoThemeMode Theme)
{
    public async Task<ApiResult<byte[]>> GetAsync(CancellationToken ct)
    {
        var request = new RestRequest("/oidc/clients/{id}/logo")
            .AddUrlSegment("id", Id)
            .AddQueryParameter("light", Theme == LogoThemeMode.Light)
            ;
        var response = await PocketId.Api.ExecuteAsync(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.RawBytes);
        }
        return response.Nok<byte[]>();
    }

    public async Task<ApiResult<int>> AmendAsync(byte[] content, string filename, string mimetype, CancellationToken ct)
    {
        var request = new RestRequest("/oidc/clients/{id}/logo")
            .AddUrlSegment("id", Id)
            .AddQueryParameter("light", Theme == LogoThemeMode.Light)
            .AddFile("file", content, filename, mimetype)
            ;
        var response = await PocketId.Api.ExecutePostAsync(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(0);
        }
        return response.Nok<int>(response.Content);

    }

    public async Task<ApiResult<int>> DeleteAsync(CancellationToken ct)
    {
        var request = new RestRequest("/oidc/clients/{id}/logo")
            .AddUrlSegment("id", Id)
            .AddQueryParameter("light", Theme == LogoThemeMode.Light)
            ;
        var response = await PocketId.Api.ExecuteDeleteAsync(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(0);
        }
        return response.Nok<int>(response.Content);
    }
}
