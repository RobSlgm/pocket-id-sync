using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PocketIdSync.Apis;
using PocketIdSync.ModelSpecs;

namespace PocketIdSync.Sync;

interface IConfigStoreOidcClient
{
    Task<int> LoadAsync(List<OidcClientSyncItem> clients, bool existingOnly = false, CancellationToken ct = default);
    Task<int> LoadAsync(List<OidcClientSyncItem> clients, string filename, string? ns = null, CancellationToken ct = default);

    Task<ConfigStoreFile> ReadLogoAsync(OidcClientSyncItem client, LogoThemeMode theme, CancellationToken ct = default);

    /// <summary>
    /// Convert remote definition to local merged configuration
    /// </summary>
    /// <param name="client"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<int> SynchronizeAsync(OidcClientSyncItem client, CancellationToken ct = default);

    Task<int> WriteAsync<T>(OidcClientSyncItem client, T data, CancellationToken ct = default) where T : IKubernetes;

    Task<int> WriteLogoAsync(OidcClientSyncItem client, byte[] data, string filename, LogoThemeMode theme, CancellationToken ct = default);
}
