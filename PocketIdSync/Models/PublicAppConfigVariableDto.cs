namespace PocketIdSync.Models;

class PublicAppConfigVariableDto
{
    public string? Key { get; set; }
    public string? Type { get; set; }   // this value is not set by the API (with one exception) [PocketId v2.5.0]
    public string? Value { get; set; }
}


sealed class AppConfigVariableDto : PublicAppConfigVariableDto
{
    public bool? IsPublic { get; set; }   // this value is not set by the API [PocketId v2.5.0]
}
