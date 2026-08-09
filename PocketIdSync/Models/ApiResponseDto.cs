using System;
using Generator.Equals;

namespace PocketIdSync.Models;

[Equatable]
partial class ApiResponseDto
{
    [IgnoreEquality]
    public DateTimeOffset? CreatedAt { get; set; }

    [IgnoreEquality]
    public string? Id { get; set; }

    public string? Name { get; set; }

    [UnorderedEquality]
    public ApiPermissionResponseDto[] Permissions { get; set; } = [];

    public string? Resource { get; set; }
}
