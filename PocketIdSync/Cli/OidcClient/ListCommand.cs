using System.Threading.Tasks;
using DotMake.CommandLine;
using PocketIdSync.Apis;
using PocketIdSync.Models;
using PocketIdSync.Utils;
using Spectre.Console;

namespace PocketIdSync.Cli.OidcClient;


[CliCommand(
   Description = "List Oidc client configurations",
   Name = "list",
   Parent = typeof(OidcClientCommand)
)]
sealed class ListCommand : AuthorizationCommandBase
{
    private readonly JsonHelper JsonHelper;

    public ListCommand(JsonHelper jsonHelper)
    {
        JsonHelper = jsonHelper;
    }

    [CliOption(Description = "Output format", Alias = "o", AllowedValues = ["json", "console"], Arity = CliArgumentArity.ZeroOrOne)]
    public string Output { get; set; } = "console";

    public async Task<int> RunAsync(CliContext context)
    {
        var pocketId = new PocketIdClient(PocketIdUri, ApiKey);
        var clients = await pocketId.OidcClients.ListAsync(context.CancellationToken);
        if (!clients.IsSuccessful)
        {
            AnsiConsole.MarkupLine($"[red]✗ Pocket ID call {clients.Uri} failed: {clients.Status}[/]");
            return ExitCode.BadRequest;
        }
        switch (Output)
        {
            // e.g. for oidc-client list -o json | jq '[.[] | {id: .id, name: .name}]'
            case "json":
                JsonHelper.WriteConsole(clients.Data);
                break;
            default:
                var table = BuildTable(clients.Data);
                AnsiConsole.Write(table);
                break;
        }
        return ExitCode.Success;
    }

    private static Table BuildTable(OidcClientWithAllowedGroupsCountDto[]? data)
    {
        var table = new Table();
        table.AddColumn("Id");
        table.AddColumn("Name");
        foreach (var client in data ?? [])
        {
            table.AddRow(client.Id ?? "", client.Name ?? "");
        }
        return table;
    }
}
