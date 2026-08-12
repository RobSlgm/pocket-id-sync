namespace PocketIdSync.Models;

class OidcClientWithAllowedGroupsDto : OidcClientDto
{
    public UserGroupMinimalDto[] AllowedUserGroups { get; set; } = [];
}


sealed class OidcClientCompleteDto : OidcClientWithAllowedGroupsDto
{
    public ApiPermissionMinimalDto[]? UserDelegatedPermissions { get; set; }
    public ApiPermissionMinimalDto[]? ClientPermissions { get; set; }
}
