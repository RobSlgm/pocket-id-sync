using System.Text.Json.Serialization;

namespace PocketIdSync.Cli.ApplicationImages;

[JsonConverter(typeof(ApplicationImageType))]
public enum ApplicationImageType
{
    All,
    Background,
    LogoLight,
    LogoDark,
    Favicon,
    Email,
    DefaultProfile,
}
