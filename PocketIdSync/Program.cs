using System;
using System.Threading;
using System.Threading.Tasks;
using DotMake.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using PocketIdSync.Apis;
using PocketIdSync.Cli;
using PocketIdSync.Utils;
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
        int exitCode;
        try
        {
            DotMake.CommandLine.Cli.Ext.ConfigureServices(services =>
            {
                services.AddTransient<OidcClientsApi>();
                services.AddTransient<UserGroupsApi>();
                services.AddSingleton<JsonHelper>();
                services.AddSingleton<YamlHelper>();
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
