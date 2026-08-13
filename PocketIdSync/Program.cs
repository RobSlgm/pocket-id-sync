using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DotMake.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PocketIdSync.Apis;
using PocketIdSync.Apis.OidcClients;
using PocketIdSync.Apis.UserGroups;
using PocketIdSync.Cli;
using PocketIdSync.Utils;
using Polly;
using Spectre.Console;

namespace PocketIdSync;

class Program
{
    static async Task<int> Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            AnsiConsole.WriteLine("Canceling with Ctrl+C requested ...");
            cts.Cancel();
            e.Cancel = true;
        };
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .AddEnvironmentVariables()
            .Build();
        foreach (var entry in config.AsEnumerable())
        {
            if (entry.Value != null)
            {
                Environment.SetEnvironmentVariable(entry.Key, entry.Value);
            }
        }
        int exitCode;
        try
        {
            DotMake.CommandLine.Cli.Ext.ConfigureServices(services =>
            {
                services.AddTransient<OidcClientsApi>();
                services.AddTransient<UserGroupsApi>();
                services.AddSingleton<JsonHelper>();
                services.AddSingleton<YamlHelper>();
                services.AddHttpClient(nameof(PocketIdClient)).AddStandardResilienceHandler(options =>
                {
                    options.Retry.ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .HandleResult(res =>
                            res.StatusCode == System.Net.HttpStatusCode.UpgradeRequired ||
                            (int)res.StatusCode >= 500 ||
                            res.StatusCode == System.Net.HttpStatusCode.RequestTimeout ||
                            res.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        .Handle<HttpRequestException>();
                    options.Retry.Delay = TimeSpan.FromSeconds(2);
                    options.Retry.UseJitter = true;
                    options.Retry.BackoffType = DelayBackoffType.Exponential;
                });
            });
            exitCode = await DotMake.CommandLine.Cli.RunAsync<RootCommand>(args, cancellationToken: cts.Token);
        }
        catch (Exception e)
        {
            AnsiConsole.MarkupLine($"[bold red]✗ Unhandled error: {e.Message}[/]");
            return ExitCode.FatalError;
        }
        return exitCode;
    }
}
