using System;

namespace PocketIdSync.Models;

sealed class ApiKeyRenewDto
{
    public required DateTimeOffset ExpiresAt { get; set; }
}
