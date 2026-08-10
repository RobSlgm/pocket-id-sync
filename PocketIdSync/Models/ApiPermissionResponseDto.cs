using Generator.Equals;
using PocketIdSync.Utils;

namespace PocketIdSync.Models;

[Equatable]
partial class ApiPermissionInputDto
{
    public string? Key { get; set; }
    public string? Name { get; set; }

    [CustomEquality(typeof(StringEmptyEqualityComparer))]
    public string? Description { get; set; }
}

[Equatable]
sealed partial class ApiPermissionResponseDto
{
    public string? Id { get; set; }

    [CustomEquality(typeof(StringEmptyEqualityComparer))]
    public string? Description { get; set; }
    public string? Key { get; set; }
    public string? Name { get; set; }
}

[Equatable]
sealed partial class ApiPermissionsUpdateDto
{
    [UnorderedEquality]
    public ApiPermissionInputDto[]? Permissions;
}

