using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotMake.CommandLine;
using PocketIdSync.Apis;
using Spectre.Console;

namespace PocketIdSync.Cli.AppApis;

abstract class AppApiIdentityCommandBase : AuthorizationCommandBase
{
    [CliArgument(Description = "Application API Id", Required = false, Arity = CliArgumentArity.ZeroOrOne)]
    public string? ApiId { get; set; }

    [CliOption(Description = "Resource", Required = false)]
    public string? Resource { get; set; }

    public static async Task<(int ExitCode, string? Id)> FindAppApi(PocketIdClient pocketId, string? ApiId, string? Resource, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(ApiId))
        {
            if (string.IsNullOrEmpty(Resource))
            {
                AnsiConsole.MarkupLine($"[red]✗ Either the application API Id or resource uri is required[/]");
                return (ExitCode.BadRequest, null);
            }
            var appapis = await pocketId.AppApis.ListAsync(ct);
            if (!appapis.IsSuccessful)
            {
                AnsiConsole.MarkupLine($"[red]✗ Pocket ID application API search failed[/]");
                return (ExitCode.FatalError, null);
            }
            ApiId = appapis.Data?.FirstOrDefault(u => string.Equals(u.Resource, Resource, System.StringComparison.OrdinalIgnoreCase))?.Id;
            if (ApiId is null)
            {
                AnsiConsole.MarkupLine($"[red]✗ Pocket ID application API with resource URI=[bold]{Resource}[/] not found[/]");
                return (ExitCode.BadRequest, null);
            }
        }
        return (ExitCode.Success, ApiId);
    }
}
