using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.Sync;

namespace PocketIdSync.Apis.ApplicationImages;

sealed class BackgroundImageApi(PocketIdClient PocketId) : ApplicationImagesBase
{
    static readonly string Uri = "/application-images/background";

    public async Task<ApiResult<byte[]>> GetAsync(CancellationToken ct) => await GetAsync(PocketId, Uri, ct);
    public async Task<ApiResult<int>> PutAsync(ConfigStoreFile data, CancellationToken ct) => await PutAsync(PocketId, Uri, data, ct);
}
