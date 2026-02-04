
using System.Text.Json.Serialization;
using PocketIdSync.ModelSpecs;

namespace PocketIdSync.Models;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
[JsonSerializable(typeof(CustomClaimDto))]
[JsonSerializable(typeof(OidcClientCredentialsDto))]
[JsonSerializable(typeof(OidcClientDto))]
[JsonSerializable(typeof(OidcClientUpdateDto))]
[JsonSerializable(typeof(OidcClientCreateDto))]
[JsonSerializable(typeof(OidcClientMetaDataDto))]
[JsonSerializable(typeof(OidcClientFederatedIdentityDto))]
[JsonSerializable(typeof(OidcClientWithAllowedGroupsCountDto))]
[JsonSerializable(typeof(OidcClientWithAllowedGroupsDto))]
[JsonSerializable(typeof(UserDto))]
[JsonSerializable(typeof(UserGroupDto))]
[JsonSerializable(typeof(UserGroupSpec))]
[JsonSerializable(typeof(OidcClientKind))]
[JsonSerializable(typeof(UserGroupMinimalDto))]
[JsonSerializable(typeof(UpdateAllowedUserGroupsDto))]
[JsonSerializable(typeof(SecretDto))]
[JsonSerializable(typeof(Pagination))]
[JsonSerializable(typeof(Paginated<OidcClientWithAllowedGroupsCountDto>))]
[JsonSerializable(typeof(Paginated<UserGroupMinimalDto>))]
[JsonSerializable(typeof(VersionInfoDto))]
[JsonSerializable(typeof(ApiKeyDto))]
[JsonSerializable(typeof(ApiKeyCreateDto))]
[JsonSerializable(typeof(ApiKeyResponseDto))]
[JsonSerializable(typeof(ApiKeyRenewDto))]
[JsonSerializable(typeof(Paginated<ApiKeyDto>))]
sealed partial class SourceGenerationContext : JsonSerializerContext { }
