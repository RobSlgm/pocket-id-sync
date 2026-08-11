namespace PocketIdSync.Models;

class OidcClientWithAllowedGroupsDto : OidcClientDto
{
    public UserGroupMinimalDto[] AllowedUserGroups { get; set; } = [];
}


sealed class OidcClientCompleteDto : OidcClientWithAllowedGroupsDto
{
    public string[]? UserDelegatedPermissionIds { get; set; }
    public string[]? ClientPermissionIds { get; set; }
}
