using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DotMake.CommandLine;
using PocketIdSync.Apis;
using PocketIdSync.Utils;
using Spectre.Console;

namespace PocketIdSync.Cli.ApiKeys;

[CliCommand(
   Description = "Delete Pocket ID API key",
   Name = "delete",
   Parent = typeof(ApiKeysCommand)
)]
sealed class DeleteCommand(JsonHelper JsonHelper, IHttpClientFactory HttpClientFactory) : ApiKeyIdentificationCommandBase
{
    [CliOption(Description = "Verbose", Alias = "v")]
    public bool Verbose { get; set; }

    public async Task<int> RunAsync(CliContext context)
    {
        if (string.IsNullOrEmpty(ApiKeyName) == string.IsNullOrEmpty(ApiKeyId))
        {
            AnsiConsole.MarkupLine($"[red]✗ Either Id OR Name of API key is required[/]");
            return ExitCode.BadRequest;
        }
        var pocketId = HttpClientFactory.Connect(PocketIdUri, ApiKey);
        if (!string.IsNullOrEmpty(ApiKeyName))
        {
            var clients = await pocketId.ApiKeys.ListAsync(context.CancellationToken);
            if (!clients.IsSuccessful)
            {
                AnsiConsole.MarkupLine($"[red]✗ Pocket ID call {clients.Uri} failed: {clients.Status}[/]");
                return ExitCode.BadRequest;
            }
            var hit = clients.Data?.FirstOrDefault(c => string.Equals(c.Name, ApiKeyName, System.StringComparison.OrdinalIgnoreCase));
            if (hit is null)
            {
                AnsiConsole.MarkupLine($"[Orange1]✓ No API Key with name {ApiKeyName} found[/]");
                return ExitCode.Success;
            }
            if (Verbose)
            {
                AnsiConsole.MarkupLine($"[Orange1]- Name {ApiKeyName} found with Id={hit.Id}[/]");
                JsonHelper.WriteConsole(hit);
            }
            ApiKeyId = hit.Id;
        }
        if (string.IsNullOrEmpty(ApiKeyId))
        {
            AnsiConsole.MarkupLine($"[red]✗ No API Key Id supplied[/]");
            return ExitCode.BadRequest;
        }
        var response = await pocketId.ApiKeys.Id(ApiKeyId).RevokeAsync(context.CancellationToken);
        if (!response.IsSuccessful)
        {
            AnsiConsole.MarkupLine($"[red]✗ Failed to delete API Key {ApiKeyId}: {response.Status} {response.ErrorMessage}[/]");
            return ExitCode.FatalError;
        }

        AnsiConsole.MarkupLine($"[Green]✓ Key {ApiKeyName??ApiKeyId} removed[/]");
        return ExitCode.Success;
    }
}
