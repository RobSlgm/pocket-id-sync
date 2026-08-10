using Generator.Equals;
using PocketIdSync.Models;

namespace PocketIdSync.ModelSpecs;

[Equatable]
sealed partial class AppApiSpec
{
    // public string? Id { get; set; }
    public string? Resource { get; set; }

    public string? Name { get; set; }

    [UnorderedEquality]
    public ApiPermissionInputDto[] Permissions { get; set; } = [];
}


sealed class AppApiKind : KubernetesSpec<AppApiSpec> { }
