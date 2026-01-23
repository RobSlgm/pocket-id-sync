namespace PocketIdSync.Models;

class OidcClientUpdateDto
{
    /// <summary>
    /// max=50
    /// </summary>
    public required string Name { get; set; }

    public string[] CallbackURLs { get; set; } = [];

    public string[] LogoutCallbackURLs { get; set; } = [];

    public bool IsPublic { get; set; }

    public bool PkceEnabled { get; set; }

    public bool RequiresReauthentication { get; set; }

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
}
