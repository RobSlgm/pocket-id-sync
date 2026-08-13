using System.Net.Http;
using System.Threading.Tasks;
using DotMake.CommandLine;
using PocketIdSync.Apis;
using PocketIdSync.ModelSpecs;
using PocketIdSync.Utils;
using Spectre.Console;

namespace PocketIdSync.Cli.Apis;


[CliCommand(
    Description = "Get OIDC client API definition from Pocket ID",
    Name = "get",
    Parent = typeof(ApiCommand)
)]
sealed class GetCommand(JsonHelper JsonHelper, YamlHelper Yaml, IHttpClientFactory HttpClientFactory) : ApiIdentityCommandBase
{
    [CliOption(Description = "Namespace, used in YAML generation", Alias = "ns", Hidden = true)]
    public string Namespace { get; set; } = "default";

    [CliOption(Description = "Output format", Alias = "o", AllowedValues = ["json", "yaml", "console"], Arity = CliArgumentArity.ZeroOrOne)]
    public string Output { get; set; } = "console";

    public async Task<int> RunAsync(CliContext context)
    {
        var pocketId = HttpClientFactory.Connect(PocketIdUri, ApiKey);
        var findApi = await FindApiAsync(pocketId, ApiId, Resource, context.CancellationToken);
        if (findApi.ExitCode != ExitCode.Success || string.IsNullOrEmpty(findApi.Id))
        {
            return findApi.ExitCode;
        }
        ApiId = findApi.Id;
        var api = await pocketId.Apis.Id(ApiId).GetAsync(context.CancellationToken);
        if (!api.IsSuccessful)
        {
            AnsiConsole.MarkupLine($"[red]✗ Pocket ID call {api.Uri} failed: {api.Status}[/]");
            return ExitCode.BadRequest;
        }
        if (api.Data is null)
        {
            AnsiConsole.MarkupLine($"[red]✗ Pocket ID OIDC client API definition [bold]{ApiId}[/] not found[/]");
            return ExitCode.BadRequest;
        }
        switch (Output)
        {
            default:
            case "json":
                JsonHelper.WriteConsole(api.Data);
                break;

            case "yaml":
                AnsiConsole.WriteLine(Yaml.Write(api.Data.ToKind()));
                break;
        }

        return ExitCode.Success;
    }
}
