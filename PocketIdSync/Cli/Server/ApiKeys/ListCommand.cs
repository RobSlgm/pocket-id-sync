using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using DotMake.CommandLine;
using PocketIdSync.Apis;
using PocketIdSync.Models;
using PocketIdSync.Utils;
using Spectre.Console;

namespace PocketIdSync.Cli.Server.ApiKeys;


[CliCommand(
   Description = "List Pocket ID API keys",
   Name = "list",
   Parent = typeof(ApiKeyCommand)
)]
sealed class ListCommand(JsonHelper JsonHelper, IHttpClientFactory HttpClientFactory) : AuthorizationCommandBase
{
    [CliOption(Description = "Output format", Alias = "o", AllowedValues = ["json", "console"], Arity = CliArgumentArity.ZeroOrOne)]
    public string Output { get; set; } = "console";

    public async Task<int> RunAsync(CliContext context)
    {
        var pocketId = HttpClientFactory.Connect(PocketIdUri, ApiKey);
        var clients = await pocketId.ApiKeys.ListAsync(context.CancellationToken);
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

    private static Table BuildTable(ApiKeyDto[]? data)
    {
        var table = new Table();
        table.AddColumn("Id");
        table.AddColumn("Name");
        table.AddColumn("Expires");
        foreach (var client in data ?? [])
        {
            table.AddRow(client.Id ?? "", client.Name ?? "", client.ExpiresAt?.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture) ?? "");
        }
        return table;
    }
}
