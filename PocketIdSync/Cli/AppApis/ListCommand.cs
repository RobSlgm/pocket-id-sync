using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using DotMake.CommandLine;
using PocketIdSync.Apis;
using PocketIdSync.Models;
using PocketIdSync.Utils;
using Spectre.Console;

namespace PocketIdSync.Cli.AppApis;


[CliCommand(
   Description = "List application API definitions from Pocket ID",
   Name = "list",
   Parent = typeof(AppApisCommand)
)]
sealed class ListCommand(JsonHelper JsonHelper, IHttpClientFactory HttpClientFactory) : AuthorizationCommandBase
{
    [CliOption(Description = "Output format", Alias = "o", AllowedValues = ["json", "console"], Arity = CliArgumentArity.ZeroOrOne)]
    public string Output { get; set; } = "console";

    public async Task<int> RunAsync(CliContext context)
    {
        var pocketId = HttpClientFactory.Connect(PocketIdUri, ApiKey);
        var apis = await pocketId.AppApis.ListAsync(context.CancellationToken);
        if (!apis.IsSuccessful)
        {
            AnsiConsole.MarkupLine($"[red]✗ Pocket ID call {apis.Uri} failed: {apis.Status}[/]");
            return ExitCode.BadRequest;
        }
        switch (Output)
        {
            // e.g. for oidc-client list -o json | jq '[.[] | {id: .id, name: .name}]'
            case "json":
                JsonHelper.WriteConsole(apis.Data);
                break;

            default:
                var table = BuildTable(apis.Data);
                AnsiConsole.Write(table);
                break;
        }
        return ExitCode.Success;
    }

    private static Table BuildTable(ApiResponseDto[]? data)
    {
        var table = new Table();
        table.AddColumn("Resource");
        table.AddColumn("Name");
        table.AddColumn("Id");
        table.AddColumn("Created");
        foreach (var client in data ?? [])
        {
            table.AddRow(client.Resource ?? "", client.Name ?? "", client.Id ?? "", client.CreatedAt?.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture) ?? "");
        }
        return table;
    }
}
