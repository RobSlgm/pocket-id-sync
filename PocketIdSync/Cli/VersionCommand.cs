using System.Net.Http;
using System.Threading.Tasks;
using DotMake.CommandLine;
using PocketIdSync.Apis;
using PocketIdSync.Utils;
using Spectre.Console;

namespace PocketIdSync.Cli;


[CliCommand(
   Description = "Show Pocket ID version",
   Name = "version",
   Parent = typeof(RootCommand)
)]
sealed class VersionCommand(JsonHelper JsonHelper, IHttpClientFactory HttpClientFactory) : AuthorizationCommandBase
{
    [CliOption(Description = "Output format", Alias = "o", AllowedValues = ["json", "console"], Arity = CliArgumentArity.ZeroOrOne)]
    public string Output { get; set; } = "console";

    public async Task<int> RunAsync(CliContext context)
    {
        var pocketId = HttpClientFactory.Connect(PocketIdUri, ApiKey);
        var currentVersion = await pocketId.Version.GetAsync(context.CancellationToken);
        if (!currentVersion.IsSuccessful || currentVersion.Data is null)
        {
            AnsiConsole.MarkupLine($"[red]✗ Pocket ID call {currentVersion.Uri} failed: {currentVersion.Status}[/]");
            return ExitCode.BadRequest;
        }
        var version = currentVersion.Data;
        switch (Output)
        {
            case "json":
                JsonHelper.WriteConsole(version);
                break;
            default:
                if (string.Equals(version.CurrentVersion, version.LatestVersion, System.StringComparison.Ordinal))
                {
                    AnsiConsole.MarkupLine($"[bold]{AnsiMarkup.Escape(PocketIdUri)}[/] running latest version [green]{version.CurrentVersion}[/].");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[bold]{AnsiMarkup.Escape(PocketIdUri)}[/] running version [orange1]{version.CurrentVersion}[/], latest version is [blue]{version.LatestVersion}[/].");
                }
                break;
        }
        return ExitCode.Success;
    }

}
