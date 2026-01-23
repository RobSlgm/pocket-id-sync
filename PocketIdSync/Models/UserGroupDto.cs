namespace PocketIdSync.Models;

sealed class UserGroupDto : UserGroupMinimalDto
{
    public OidcClientMetaDataDto[] AllowedOidcClients { get; set; } = [];
    public UserDto[] Users { get; set; } = [];
}


sealed class UpdateAllowedUserGroupsDto
{
    public string[] UserGroupIds { get; set; } = [];
}
