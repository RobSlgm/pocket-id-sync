using PocketIdSync.ModelSpecs;
using YamlDotNet.Serialization;

namespace PocketIdSync.Models;


[YamlStaticContext]
[YamlSerializable(typeof(KubernetesBase))]
[YamlSerializable(typeof(KubernetesMetadata))]
[YamlSerializable(typeof(CustomClaimDto))]
[YamlSerializable(typeof(ApiPermission))]
[YamlSerializable(typeof(OidcClientApiKind))]
[YamlSerializable(typeof(OidcClientApiSpec))]
[YamlSerializable(typeof(ApiPermissionInputDto))]
[YamlSerializable(typeof(UserGroupKind))]
[YamlSerializable(typeof(UserGroupSpec))]
[YamlSerializable(typeof(OidcClientKind))]
[YamlSerializable(typeof(OidcClientSpec))]
[YamlSerializable(typeof(OidcClientSecretKind))]
[YamlSerializable(typeof(OidcClientSecretSpec))]
[YamlSerializable(typeof(UserDto))]
[YamlSerializable(typeof(OidcClientMetaDataDto))]
[YamlSerializable(typeof(UserGroupDto))]
[YamlSerializable(typeof(OidcClientCredentialsDto))]
[YamlSerializable(typeof(OidcClientFederatedIdentityDto))]
// [YamlSerializable(typeof(byte[]))]
sealed partial class SourceGenerationYamlContext : YamlDotNet.Serialization.StaticContext
{
}
