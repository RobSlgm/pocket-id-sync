namespace PocketIdSync.Models;

sealed class OidcClientWithAllowedGroupsCountDto : OidcClientDto
{
    public int? AllowedUserGroupsCount { get; set; }

}
