using Generator.Equals;
using PocketIdSync.Models;

namespace PocketIdSync.ModelSpecs;

[Equatable]
sealed partial class OidcClientApiSpec
{
    // public string? Id { get; set; }
    public string? Resource { get; set; }

    public string? Name { get; set; }

    [UnorderedEquality]
    public ApiPermissionInputDto[] Permissions { get; set; } = [];
}


[Equatable]
sealed partial class ApiPermission
{
    public required string Resource { get; set; }
    public required string Key { get; set; }
}


sealed class OidcClientApiKind : KubernetesSpec<OidcClientApiSpec>;
