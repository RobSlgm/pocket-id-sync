using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.Sync;
using RestSharp;

namespace PocketIdSync.Apis.ApplicationImages;

sealed class DefaultProfilePictureApi(PocketIdClient PocketId) : ApplicationImagesBase
{
    static readonly string Uri = "/application-images/default-profile-picture";

    public async Task<ApiResult<byte[]>> GetAsync(CancellationToken ct) => await GetAsync(PocketId, Uri, ct);
    public async Task<ApiResult<int>> PutAsync(ConfigStoreFile data, CancellationToken ct) => await PutAsync(PocketId, Uri, data, ct);

    public async Task<ApiResult<int>> DeleteAsync(CancellationToken ct)
    {
        var request = new RestRequest(Uri);
        var response = await PocketId.Api.ExecuteDeleteAsync(request, ct);
        if (response.IsSuccessful)
        {
            return response.Ok(0);
        }
        return response.Nok<int>(response.Content);
    }
}

