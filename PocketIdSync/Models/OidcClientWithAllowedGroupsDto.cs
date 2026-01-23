namespace PocketIdSync.Models;

sealed class OidcClientWithAllowedGroupsDto : OidcClientDto
{
    public UserGroupMinimalDto[] AllowedUserGroups { get; set; } = [];
}
