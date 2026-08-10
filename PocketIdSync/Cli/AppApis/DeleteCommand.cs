using System.Net.Http;
using System.Threading.Tasks;
using DotMake.CommandLine;
using PocketIdSync.Apis;
using Spectre.Console;

namespace PocketIdSync.Cli.AppApis;


[CliCommand(
    Description = "Delete application API definition from Pocket ID",
    Name = "delete",
    Parent = typeof(AppApisCommand)
)]
sealed class DeleteCommand(IHttpClientFactory HttpClientFactory) : AppApiIdentityCommandBase
{
    public async Task<int> RunAsync(CliContext context)
    {
        var pocketId = HttpClientFactory.Connect(PocketIdUri, ApiKey);
        var findAppApi = await FindAppApi(pocketId, ApiId, Resource, context.CancellationToken);
        if (findAppApi.ExitCode != ExitCode.Success || string.IsNullOrEmpty(findAppApi.Id))
        {
            return findAppApi.ExitCode;
        }
        ApiId = findAppApi.Id;
        var result = await pocketId.AppApis.Id(ApiId).DeleteAsync(context.CancellationToken);
        if (!result.IsSuccessful)
        {
            if (result.Status != System.Net.HttpStatusCode.NotFound)
            {
                AnsiConsole.MarkupLine($"[red]✗ Pocket ID call {result.Uri} failed: {result.Status}[/]");
                return ExitCode.BadRequest;
            }
            AnsiConsole.MarkupLine($"[Orange1]✓ Pocket ID application API [bold]{ApiId}[/] not found, nothing to do[/]");
            return ExitCode.Success;
        }
        AnsiConsole.MarkupLine($"[green]✓ Pocket ID application API [bold]{ApiId}[/] deleted[/]");
        return ExitCode.Success;
    }
}
