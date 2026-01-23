using System.Threading.Tasks;
using DotMake.CommandLine;
using PocketIdSync.Apis;
using Spectre.Console;

namespace PocketIdSync.Cli.OidcClient;


[CliCommand(
    Description = "Delete client configuration from Pocket ID",
    Name = "delete",
    Parent = typeof(OidcClientCommand)
)]
sealed class DeleteCommand : AuthorizationCommandBase
{
    [CliArgument(Description = "Oidc client Id", Required = true)]
    public required string ClientId { get; set; }

    public async Task<int> RunAsync(CliContext context)
    {
        var pocketId = new PocketIdClient(PocketIdUri, ApiKey);
        var client = await pocketId.OidcClients.Id(ClientId).DeleteAsync(context.CancellationToken);
        if (!client.IsSuccessful)
        {
            if (client.Status != System.Net.HttpStatusCode.NotFound)
            {
                AnsiConsole.MarkupLine($"[red]✗ Pocket ID call {client.Uri} failed: {client.Status}[/]");
                return ExitCode.BadRequest;
            }
            AnsiConsole.MarkupLine($"[Orange1]✓ Pocket ID OidcClient [bold]{ClientId}[/] not found, nothing to do[/]");
            return ExitCode.Success;
        }
        AnsiConsole.MarkupLine($"[green]✓ Pocket ID UserGroup [bold]{ClientId}[/] deleted[/]");
        return ExitCode.Success;
    }
}
