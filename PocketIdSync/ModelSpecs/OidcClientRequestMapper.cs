using PocketIdSync.Models;
using Riok.Mapperly.Abstractions;

namespace PocketIdSync.ModelSpecs;

[Mapper(ThrowOnMappingNullMismatch = false)]
static partial class OidcClientRequestMapper
{
    [MapperIgnoreSource(nameof(OidcClientWithAllowedGroupsDto.AllowedUserGroups))]
    [MapperIgnoreSource(nameof(OidcClientWithAllowedGroupsDto.PkceSupported))]
    [MapperIgnoreTarget(nameof(OidcClientCreateDto.LogoUrl))]
    [MapperIgnoreTarget(nameof(OidcClientCreateDto.DarkLogoUrl))]
    private static partial OidcClientCreateDto MapForCreate(OidcClientWithAllowedGroupsDto data);

    [MapperIgnoreSource(nameof(OidcClientWithAllowedGroupsDto.Id))]
    [MapperIgnoreSource(nameof(OidcClientWithAllowedGroupsDto.AllowedUserGroups))]
    [MapperIgnoreSource(nameof(OidcClientWithAllowedGroupsDto.PkceSupported))]
    [MapperIgnoreTarget(nameof(OidcClientUpdateDto.LogoUrl))]
    [MapperIgnoreTarget(nameof(OidcClientUpdateDto.DarkLogoUrl))]
    private static partial OidcClientUpdateDto MapForUpdate(OidcClientWithAllowedGroupsDto data);

    public static OidcClientUpdateDto ToUpdateRequest(this OidcClientWithAllowedGroupsDto data)
    {
        return MapForUpdate(data);
    }

    public static OidcClientCreateDto ToCreateRequest(this OidcClientWithAllowedGroupsDto data)
    {
        return MapForCreate(data);
    }
}
