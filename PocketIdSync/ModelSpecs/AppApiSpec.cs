using Generator.Equals;
using PocketIdSync.Models;

namespace PocketIdSync.ModelSpecs;

[Equatable]
sealed partial class AppApiSpec
{
    public string? Id { get; set; }

    public string? Name { get; set; }

    public string? Resource { get; set; }

    [UnorderedEquality]
    public ApiPermissionResponseDto[] Permissions { get; set; } = [];
}


sealed class AppApiKind : KubernetesSpec<AppApiSpec> { }
