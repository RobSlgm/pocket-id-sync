using System.Text.Json.Serialization;

namespace PocketIdSync.Cli.Server.Images;

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
