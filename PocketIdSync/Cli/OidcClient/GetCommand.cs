using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using DotMake.CommandLine;
using PocketIdSync.Apis;
using PocketIdSync.ModelSpecs;
using PocketIdSync.Repositories;
using PocketIdSync.Utils;
using Spectre.Console;

namespace PocketIdSync.Cli.OidcClient;


[CliCommand(
    Description = "Get client configuration from Pocket ID",
    Name = "get",
    Parent = typeof(OidcClientCommand)
)]
sealed class GetCommand(JsonHelper JsonHelper, YamlHelper Yaml, IHttpClientFactory HttpClientFactory) : AuthorizationCommandBase
{
    [CliArgument(Description = "Oidc client Id", Required = true)]
    public required string ClientId { get; set; }

    [CliOption(Description = "Download logo(s)", Alias = "logo", Required = false)]
    public bool IncludeLogos { get; set; }

    [CliOption(Description = "Output format", Alias = "o", AllowedValues = ["json", "yaml", "console"], Arity = CliArgumentArity.ZeroOrOne)]
    public string Output { get; set; } = "console";

    public async Task<int> RunAsync(CliContext context)
    {
        var pocketId = HttpClientFactory.Connect(PocketIdUri, ApiKey);
        var oidcClientRepository = new OidcClientRepository();
        var client = await oidcClientRepository.GetAsync(pocketId, ClientId, context.CancellationToken);
        if (!client.IsSuccessful)
        {
            AnsiConsole.MarkupLine($"[red]✗ Pocket ID call {client.Uri} failed: {client.Status}[/]");
            return ExitCode.BadRequest;
        }
        if (client.Data is null)
        {
            AnsiConsole.MarkupLine($"[red]✗ Pocket ID OidcClient [bold]{ClientId}[/] not found[/]");
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
        if (IncludeLogos == true)
        {
            foreach (var theme in Enum.GetValues<LogoThemeMode>())
            {
                var hasLogo = theme == LogoThemeMode.Light ? client.Data.HasLogo : client.Data.HasDarkLogo;
                if (hasLogo == true)
                {
                    var logoResponse = await pocketId.OidcClients.Id(client.Data.Id!).Logo(theme).GetAsync(context.CancellationToken);
                    if (logoResponse.Data is not null)
                    {
                        var extension = MimeTypeUtil.FromMimeType(logoResponse.MimeType) ?? ".unknown";
                        AnsiConsole.MarkupLine($"Logo {client.Data.Id}{(theme == LogoThemeMode.Light ? "" : "-dark")}{extension} (from {logoResponse.MimeType})");
                        await File.WriteAllBytesAsync(Path.Combine(".", $"{client.Data.Id}{(theme == LogoThemeMode.Light ? "" : "-dark")}{extension}"), logoResponse.Data, context.CancellationToken);
                    }
                }
            }
        }
        return ExitCode.Success;
    }
}
