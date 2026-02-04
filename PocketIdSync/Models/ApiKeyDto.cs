using System;
using Generator.Equals;

namespace PocketIdSync.Models;

[Equatable]
sealed partial class ApiKeyDto
{
    public DateTimeOffset? CreatedAt { get; set; }

    public string? Description { get; set; }

    public bool? ExpirationEmailSent { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public string? Id { get; set; }

    [IgnoreEquality]
    public DateTimeOffset? LastUsedAt { get; set; }

    public string? Name { get; set; }
}
