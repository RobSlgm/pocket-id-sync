namespace PocketIdSync.Models;

sealed class VersionInfoDto
{
    public string? LatestVersion { get; set; }
    public string? CurrentVersion { get; set; }
}
