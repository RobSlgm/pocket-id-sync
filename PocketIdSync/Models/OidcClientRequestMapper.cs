using System.Linq;
using Riok.Mapperly.Abstractions;

namespace PocketIdSync.Models;

[Mapper(ThrowOnMappingNullMismatch = false)]
static partial class OidcClientRequestMapper
{
    [MapperIgnoreSource(nameof(OidcClientCompleteDto.AllowedUserGroups))]
    [MapperIgnoreSource(nameof(OidcClientCompleteDto.UserDelegatedPermissions))]
    [MapperIgnoreSource(nameof(OidcClientCompleteDto.ClientPermissions))]
    [MapperIgnoreSource(nameof(OidcClientCompleteDto.PkceSupported))]
    [MapperIgnoreTarget(nameof(OidcClientCreateDto.LogoUrl))]
    [MapperIgnoreTarget(nameof(OidcClientCreateDto.DarkLogoUrl))]
    private static partial OidcClientCreateDto MapForCreate(OidcClientCompleteDto data);

    [MapperIgnoreSource(nameof(OidcClientCompleteDto.Id))]
    [MapperIgnoreSource(nameof(OidcClientCompleteDto.AllowedUserGroups))]
    [MapperIgnoreSource(nameof(OidcClientCompleteDto.UserDelegatedPermissions))]
    [MapperIgnoreSource(nameof(OidcClientCompleteDto.ClientPermissions))]
    [MapperIgnoreSource(nameof(OidcClientCompleteDto.PkceSupported))]
    [MapperIgnoreTarget(nameof(OidcClientUpdateDto.LogoUrl))]
    [MapperIgnoreTarget(nameof(OidcClientUpdateDto.DarkLogoUrl))]
    private static partial OidcClientUpdateDto MapForUpdate(OidcClientCompleteDto data);

    public static OidcClientUpdateDto ToUpdateRequest(this OidcClientCompleteDto data)
    {
        return MapForUpdate(data);
    }

    public static OidcClientCreateDto ToCreateRequest(this OidcClientCompleteDto data)
    {
        return MapForCreate(data);
    }

    public static ClientApiAccessUpdateDto ToClientApiAccessUpdateRequest(this OidcClientCompleteDto data)
    {
        return new ClientApiAccessUpdateDto
        {
            ClientPermissionIds = data.ClientPermissions?.Select(p => p.Id!).ToArray(),
            UserDelegatedPermissionIds = data.UserDelegatedPermissions?.Select(p => p.Id!).ToArray(),
        };
    }
}
