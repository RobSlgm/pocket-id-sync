using Generator.Equals;
using PocketIdSync.Models;

namespace PocketIdSync.ModelSpecs;

[Equatable]
sealed partial class UserGroupSpec
{
    public string? FriendlyName { get; set; }
    public string? Name { get; set; }

    [UnorderedEquality]
    public CustomClaimDto[] CustomClaims { get; set; } = [];
}


sealed class UserGroupKind : KubernetesSpec<UserGroupSpec> { }
