using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using DotMake.CommandLine;
using PocketIdSync.Apis;
using PocketIdSync.Models;
using PocketIdSync.Utils;
using Spectre.Console;

namespace PocketIdSync.Cli.ApiKeys;

[CliCommand(
   Description = "Create new Pocket ID API key",
   Name = "create",
   Hidden = true,
   Parent = typeof(ApiKeysCommand)
)]
sealed class CreateCommand(JsonHelper JsonHelper, IHttpClientFactory HttpClientFactory) : AuthorizationCommandBase
{
    [CliArgument(Description = "API Key Name")]
    public required string ApiKeyName { get; set; }

    [CliOption(Description = "API Key description", Name = "desc", Required = false)]
    public string? ApiKeyDescription { get; set; }

    [CliOption(Description = "API Key duration in days", Name = "expiry", Required = false)]
    public int ApiKeyExpiration { get; set; } = 1;

    [CliOption(Description = "Verbose", Alias = "v")]
    public bool Verbose { get; set; }

    public async Task<int> RunAsync(CliContext context)
    {
        if (ApiKeyExpiration < 0 || ApiKeyExpiration > 365)
        {
            AnsiConsole.MarkupLine($"[red]✗ Expiration in days must be between 1 and 365[/]");
            return ExitCode.BadRequest;
        }
        var pocketId = HttpClientFactory.Connect(PocketIdUri, ApiKey);
        var response = await pocketId.ApiKeys.PostAsync(new ApiKeyCreateDto
        {
            Name = ApiKeyName,
            Description = ApiKeyDescription,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(ApiKeyExpiration),
        }, context.CancellationToken);
        if (!response.IsSuccessful || response.Data is null)
        {
            AnsiConsole.MarkupLine($"[red]✗ Failed to create API Key {ApiKeyName}: {response.Status} {response.ErrorMessage}[/]");
            return ExitCode.FatalError;
        }
        if (Verbose)
        {
            JsonHelper.WriteConsole(response.Data);
        }
        AnsiConsole.MarkupLine($"[Green]✓ Key {response.Data.ApiKey?.Name} created {response.Data.Token} valid until {response.Data.ApiKey?.ExpiresAt?.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)}[/]");
        return ExitCode.Success;
    }
}
