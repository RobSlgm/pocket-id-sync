using Generator.Equals;

namespace PocketIdSync.Models;

[Equatable]
sealed partial class OidcClientFederatedIdentityDto
{
    public string? Audience { get; set; }

    public string? Issuer { get; set; }

    public string? Jwks { get; set; }

    public string? Subject { get; set; }

    public bool? ReplayProtection { get; set; }
}
