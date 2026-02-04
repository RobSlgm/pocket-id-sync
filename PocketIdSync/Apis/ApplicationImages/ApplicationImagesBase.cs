using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.Sync;
using RestSharp;

namespace PocketIdSync.Apis.ApplicationImages;

class ApplicationImagesBase
{
    public static async Task<ApiResult<byte[]>> GetAsync(PocketIdClient pocketId, string url, CancellationToken ct) => await GetAsync(pocketId, new RestRequest(url), ct);

    public static async Task<ApiResult<byte[]>> GetAsync(PocketIdClient pocketId, string url, LogoThemeMode theme, CancellationToken ct) => await GetAsync(pocketId, new RestRequest(url).AddQueryParameter("light", theme == LogoThemeMode.Light), ct);

    private static async Task<ApiResult<byte[]>> GetAsync(PocketIdClient pocketId, RestRequest request, CancellationToken ct)
    {
        var response = await pocketId.Api.ExecuteAsync(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(response.RawBytes);
        }
        return response.Nok<byte[]>();
    }

    public static async Task<ApiResult<int>> PutAsync(PocketIdClient pocketId, string url, ConfigStoreFile data, CancellationToken ct) => await PutAsync(pocketId, new RestRequest(url).AddFile("file", data.Content!, data.Filename!, data.Mimetype), ct);

    public static async Task<ApiResult<int>> PutAsync(PocketIdClient pocketId, string url, ConfigStoreFile data, LogoThemeMode theme, CancellationToken ct) => await PutAsync(pocketId, new RestRequest(url).AddQueryParameter("light", theme == LogoThemeMode.Light).AddFile("file", data.Content!, data.Filename!, data.Mimetype), ct);

    private static async Task<ApiResult<int>> PutAsync(PocketIdClient pocketId, RestRequest request, CancellationToken ct)
    {
        var response = await pocketId.Api.ExecutePutAsync(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(0);
        }
        return response.Nok<int>(response.Content);
    }
}
