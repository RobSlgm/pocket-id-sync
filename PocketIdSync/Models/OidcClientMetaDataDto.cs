namespace PocketIdSync.Models;

sealed class OidcClientMetaDataDto
{
    public bool? HasDarkLogo { get; set; }
    public bool? HasLogo { get; set; }
    public string? Id { get; set; }
    public string? LaunchURL { get; set; }
    public string? Name { get; set; }
    public bool? RequiresReauthentication { get; set; }
}
