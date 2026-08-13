using System.Net.Http;
using System.Threading.Tasks;
using DotMake.CommandLine;
using PocketIdSync.Apis;
using Spectre.Console;

namespace PocketIdSync.Cli.Apis;


[CliCommand(
    Description = "Delete OIDC client API definition from Pocket ID",
    Name = "delete",
    Parent = typeof(ApiCommand)
)]
sealed class DeleteCommand(IHttpClientFactory HttpClientFactory) : ApiIdentityCommandBase
{
    public async Task<int> RunAsync(CliContext context)
    {
        var pocketId = HttpClientFactory.Connect(PocketIdUri, ApiKey);
        var findApi = await FindApiAsync(pocketId, ApiId, Resource, context.CancellationToken);
        if (findApi.ExitCode != ExitCode.Success || string.IsNullOrEmpty(findApi.Id))
        {
            return findApi.ExitCode;
        }
        ApiId = findApi.Id;
        var result = await pocketId.Apis.Id(ApiId).DeleteAsync(context.CancellationToken);
        if (!result.IsSuccessful)
        {
            if (result.Status != System.Net.HttpStatusCode.NotFound)
            {
                AnsiConsole.MarkupLine($"[red]✗ Pocket ID call {result.Uri} failed: {result.Status}[/]");
                return ExitCode.BadRequest;
            }
            AnsiConsole.MarkupLine($"[Orange1]✓ Pocket ID OIDC client API [bold]{ApiId}[/] not found, nothing to do[/]");
            return ExitCode.Success;
        }
        AnsiConsole.MarkupLine($"[green]✓ Pocket ID OIDC client API [bold]{ApiId}[/] deleted[/]");
        return ExitCode.Success;
    }
}
