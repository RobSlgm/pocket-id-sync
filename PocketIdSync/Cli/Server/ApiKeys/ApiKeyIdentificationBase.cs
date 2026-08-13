using DotMake.CommandLine;

namespace PocketIdSync.Cli.Server.ApiKeys;

abstract class ApiKeyIdentificationCommandBase : AuthorizationCommandBase
{
    [CliOption(Description = "API Key Name", Name = "name", Alias = "n", Required = false)]
    public string? ApiKeyName { get; set; }

    [CliOption(Description = "API Key Id", Name = "id", Alias = "", Required = false)]
    public string? ApiKeyId { get; set; }
}
