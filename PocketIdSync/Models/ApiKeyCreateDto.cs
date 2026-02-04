using System;

namespace PocketIdSync.Models;

sealed class ApiKeyCreateDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required DateTimeOffset ExpiresAt { get; set; }
}
