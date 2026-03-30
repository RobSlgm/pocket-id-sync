using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DotMake.CommandLine;
using PocketIdSync.Apis;
using PocketIdSync.Models;
using PocketIdSync.Utils;
using Spectre.Console;

namespace PocketIdSync.Cli.ApplicationConfiguration;


[CliCommand(
   Description = "List public application configurations",
   Name = "list",
   Parent = typeof(ApplicationConfigurationCommand)
)]
sealed class ListCommand(JsonHelper JsonHelper, IHttpClientFactory HttpClientFactory) : AuthorizationCommandBase
{
    [CliOption(Description = "Output format", Alias = "o", AllowedValues = ["json", "console"], Arity = CliArgumentArity.ZeroOrOne)]
    public string Output { get; set; } = "console";

    [CliOption(Description = "Include private configuration", Alias = "p")]
    public bool IncludePrivate { get; set; } = false;

    public async Task<int> RunAsync(CliContext context)
    {
        var pocketId = HttpClientFactory.Connect(PocketIdUri, ApiKey);
        var configFull = await pocketId.ApplicationConfiguration.ListAllAsync(context.CancellationToken);
        if (!configFull.IsSuccessful)
        {
            AnsiConsole.MarkupLine($"[red]✗ Pocket ID call {configFull.Uri} failed: {configFull.Status}[/]");
            return ExitCode.BadRequest;
        }
        var configPublic = await pocketId.ApplicationConfiguration.ListAsync(context.CancellationToken);
        if (!configPublic.IsSuccessful)
        {
            AnsiConsole.MarkupLine($"[red]✗ Pocket ID call {configPublic.Uri} failed: {configPublic.Status}[/]");
            return ExitCode.BadRequest;
        }
        var configuration = new List<AppConfigVariableDto>(configFull.Data ?? []);
        foreach (var pci in configPublic.Data ?? [])
        {
            var ci = configuration.FirstOrDefault(c => c.Key?.Equals(pci.Key, System.StringComparison.Ordinal) == true);
            if (ci is null)
            {
                configuration.Add(new AppConfigVariableDto { Key = pci.Key, Value = pci.Value, IsPublic = true, });
            }
            else
            {
                ci.IsPublic = true;
            }
        }
        switch (Output)
        {
            case "json":
                JsonHelper.WriteConsole(configuration);
                break;
            default:
                var table = BuildTable(configuration);
                AnsiConsole.Write(table);
                break;
        }
        return ExitCode.Success;
    }

    private static Table BuildTable(List<AppConfigVariableDto> data)
    {
        var table = new Table();
        table.AddColumn("Property");
        // table.AddColumn("Type");
        table.AddColumn("Value");
        table.AddColumn("Public");
        foreach (var client in data)
        {
            table.AddRow(new Text(client.Key ?? ""), new Text(client.Value ?? ""), new Text((client.IsPublic ?? false) ? "Public" : ""));
        }
        return table;
    }
}
