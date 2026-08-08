using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using DotMake.CommandLine;
using PocketIdSync.Apis;
using PocketIdSync.ModelSpecs;
using PocketIdSync.Utils;
using Spectre.Console;

namespace PocketIdSync.Cli.AppApis;


[CliCommand(
    Description = "Get application API definition from Pocket ID",
    Name = "get",
    Parent = typeof(AppApisCommand)
)]
sealed class GetCommand(JsonHelper JsonHelper, YamlHelper Yaml, IHttpClientFactory HttpClientFactory) : AuthorizationCommandBase
{
    [CliArgument(Description = "Application API Id", Required = true)]
    public required string ApiId { get; set; }

    [CliOption(Description = "Output format", Alias = "o", AllowedValues = ["json", "yaml", "console"], Arity = CliArgumentArity.ZeroOrOne)]
    public string Output { get; set; } = "console";

    public async Task<int> RunAsync(CliContext context)
    {
        var pocketId = HttpClientFactory.Connect(PocketIdUri, ApiKey);
        var client = await pocketId.AppApis.Id(ApiId).GetAsync(context.CancellationToken);
        if (!client.IsSuccessful)
        {
            AnsiConsole.MarkupLine($"[red]✗ Pocket ID call {client.Uri} failed: {client.Status}[/]");
            return ExitCode.BadRequest;
        }
        if (client.Data is null)
        {
            AnsiConsole.MarkupLine($"[red]✗ Pocket ID application API definition [bold]{ApiId}[/] not found[/]");
            return ExitCode.BadRequest;
        }
        switch (Output)
        {
            default:
            case "json":
                JsonHelper.WriteConsole(client.Data);
                break;

            // case "yaml":
            //     AnsiConsole.WriteLine(Yaml.Write(client.Data.ToKind()));
            //     break;
        }

        return ExitCode.Success;
    }
}
