using DotMake.CommandLine;

namespace PocketIdSync.Cli;

abstract class AuthorizationCommandBase
{
    [CliOption(Description = "Pocket ID API Uri", Alias = "uri", Recursive = true, Required = true)]
    public required string PocketIdUri { get; set; }

    [CliOption(Description = "Pocket ID API Key", Alias = "secret", Recursive = true, Required = true)]
    public required string ApiKey { get; set; }
}
