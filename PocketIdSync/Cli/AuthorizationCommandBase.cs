using System;
using DotMake.CommandLine;

namespace PocketIdSync.Cli;

abstract class AuthorizationCommandBase
{
    [CliOption(Description = "Pocket ID API Uri (or env POCKETID_URI)", Alias = "uri", Recursive = true)]
    public string PocketIdUri { get; set; } = Environment.GetEnvironmentVariable("POCKETID_URI") ?? "";

    [CliOption(Description = "Pocket ID API Key (or env POCKETID_SECRET)", Alias = "secret", Recursive = true)]
    public string ApiKey { get; set; } = Environment.GetEnvironmentVariable("POCKETID_SECRET") ?? "";
}
