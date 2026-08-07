using Generator.Equals;

namespace PocketIdSync.Models;

[Equatable]
partial class OidcClientDto
{
    public int? AccessTokenDurationMinutes { get; set; }

    [UnorderedEquality(System.StringComparison.OrdinalIgnoreCase)]
    public string[] CallbackURLs { get; set; } = [];

    public OidcClientCredentialsDto? Credentials { get; set; }

    public bool? HasDarkLogo { get; set; }

    public bool? HasLogo { get; set; }

    public string? Id { get; set; }

    public string? Description { get; set; }

    public bool? IsGroupRestricted { get; set; }

    public bool? IsPublic { get; set; }

    public string? LaunchURL { get; set; }

    [UnorderedEquality(System.StringComparison.OrdinalIgnoreCase)]
    public string[] LogoutCallbackURLs { get; set; } = [];

    public string? Name { get; set; }

    public bool? PkceEnabled { get; set; }

    public bool? PkceSupported { get; set; }

    public int? RefreshTokenDurationMinutes { get; set; }

    public bool? RequiresReauthentication { get; set; }

    public bool? RequiresPushedAuthorizationRequests { get; set; }

    public bool? SkipConsent { get; set; }
}
