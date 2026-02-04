using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.Sync;

namespace PocketIdSync.Apis.ApplicationImages;

sealed class LogoApi(PocketIdClient PocketId, LogoThemeMode Theme) : ApplicationImagesBase
{
    static readonly string Uri = "/application-images/logo";

    public async Task<ApiResult<byte[]>> GetAsync(CancellationToken ct) => await GetAsync(PocketId, Uri, Theme, ct);
    public async Task<ApiResult<int>> PutAsync(ConfigStoreFile data, CancellationToken ct) => await PutAsync(PocketId, Uri, data, Theme, ct);
}
