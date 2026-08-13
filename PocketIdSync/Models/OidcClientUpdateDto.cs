namespace PocketIdSync.Models;

class OidcClientUpdateDto
{
    /// <summary>
    /// max=50
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// max=150
    /// </summary>
    public string? Description { get; set; }

    public string[] CallbackURLs { get; set; } = [];

    public string[] LogoutCallbackURLs { get; set; } = [];

    public bool IsPublic { get; set; }

    public bool PkceEnabled { get; set; }

    public bool RequiresReauthentication { get; set; }

    public bool RequiresPushedAuthorizationRequests { get; set; }

    public bool SkipConsent { get; set; }

    public OidcClientCredentialsDto? Credentials { get; set; }

    public string? LaunchURL { get; set; }

    public bool HasLogo { get; set; }

    public bool HasDarkLogo { get; set; }

    /// <summary>
    /// External Logo URI (not supported by sync)
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// External Logo URI (not supported by sync)
    /// </summary>
    public string? DarkLogoUrl { get; set; }

    public bool IsGroupRestricted { get; set; }

    public int? AccessTokenDurationMinutes { get; set; }

    public int? RefreshTokenDurationMinutes { get; set; }

}
