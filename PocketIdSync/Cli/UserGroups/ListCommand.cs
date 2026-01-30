using System.Net.Http;
using System.Threading.Tasks;
using DotMake.CommandLine;
using PocketIdSync.Apis;
using PocketIdSync.Models;
using PocketIdSync.Utils;
using Spectre.Console;

namespace PocketIdSync.Cli.UserGroups;


[CliCommand(
   Description = "List user groups",
   Name = "list",
   Parent = typeof(UserGroupsCommand)
)]
sealed class ListCommand(JsonHelper JsonHelper, IHttpClientFactory HttpClientFactory) : AuthorizationCommandBase
{
    [CliOption(Description = "Output format", Alias = "o", AllowedValues = ["json", "console"], Arity = CliArgumentArity.ZeroOrOne)]
    public string Output { get; set; } = "console";

    public async Task<int> RunAsync(CliContext context)
    {
        var pocketId = HttpClientFactory.Connect(PocketIdUri, ApiKey);
        var usergroups = await pocketId.UserGroups.ListAsync(context.CancellationToken);
        if (!usergroups.IsSuccessful)
        {
            AnsiConsole.MarkupLine($"[red]✗ Pocket ID call {usergroups.Uri} failed: {usergroups.Status}[/]");
            return ExitCode.BadRequest;
        }
        switch (Output)
        {
            // e.g. user-groups list -o json | jq '[.[] | {id, name, friendlyName}]'
            case "json":
                JsonHelper.WriteConsole(usergroups.Data);
                break;
            default:
                var table = BuildTable(usergroups.Data);
                AnsiConsole.Write(table);
                break;
        }
        return ExitCode.Success;
    }

    private static Table BuildTable(UserGroupMinimalDto[]? data)
    {
        var table = new Table();
        table.AddColumn("Id");
        table.AddColumn("Name");
        table.AddColumn("Friendly name");
        foreach (var client in data ?? [])
        {
            table.AddRow(client.Id ?? "", client.Name ?? "", client.FriendlyName ?? "");
        }
        return table;
    }
}
