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
sealed class GetCommand(JsonHelper JsonHelper, YamlHelper Yaml, IHttpClientFactory HttpClientFactory) : AppApiIdentityCommandBase
{
    [CliOption(Description = "Namespace, used in YAML generation", Alias = "ns", Hidden = true)]
    public string Namespace { get; set; } = "default";

    [CliOption(Description = "Output format", Alias = "o", AllowedValues = ["json", "yaml", "console"], Arity = CliArgumentArity.ZeroOrOne)]
    public string Output { get; set; } = "console";

    public async Task<int> RunAsync(CliContext context)
    {
        var pocketId = HttpClientFactory.Connect(PocketIdUri, ApiKey);
        var findUserGroup = await FindAppApi(pocketId, ApiId, Resource, context.CancellationToken);
        if (findUserGroup.ExitCode != ExitCode.Success || string.IsNullOrEmpty(findUserGroup.Id))
        {
            return findUserGroup.ExitCode;
        }
        ApiId = findUserGroup.Id;
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

            case "yaml":
                AnsiConsole.WriteLine(Yaml.Write(client.Data.ToKind()));
                break;
        }

        return ExitCode.Success;
    }
}
