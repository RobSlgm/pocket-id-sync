using System.Text.Json.Serialization;

namespace PocketIdSync.Apis;

[JsonConverter(typeof(LogoThemeMode))]
enum LogoThemeMode
{
    Light,
    Dark,
}
