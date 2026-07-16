using Generator.Equals;

namespace PocketIdSync.Models;

[Equatable]
partial class ApiPermissionResponseDto
{
    public string? Description { get; set; }
    public string? Id { get; set; }
    public string? Key { get; set; }
    public string? Name { get; set; }

}
