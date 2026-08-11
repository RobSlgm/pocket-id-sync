using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DotMake.CommandLine;
using PocketIdSync.Apis;
using PocketIdSync.ModelSpecs;
using PocketIdSync.Sync;
using PocketIdSync.Utils;
using Spectre.Console;

namespace PocketIdSync.Cli.AppApis;


[CliCommand(
    Description = "Sync application API configuration",
    Name = "sync",
    ShortFormAutoGenerate = CliNameAutoGenerate.Options,
    Parent = typeof(AppApisCommand)
)]
sealed class SyncCommand(JsonHelper JsonHelper, YamlHelper Yaml, IHttpClientFactory HttpClientFactory) : SyncCommandBase
{
    public async Task<int> RunAsync(CliContext context)
    {
        var exitCode = await AnsiConsole.Status()
            .Spinner(Spinner.Known.BluePulse)
            .StartAsync("Sync application API configuration ...", async ctx => await RunAsync(context, ctx));
        return exitCode;
    }

    private ISynchronizer<AppApiSyncItem>? Initialize()
    {
        var localStore = new AppApiFilestore(StoreRoot.FullName, Yaml)
        {
        };
        if (!localStore.Exists)
        {
            AnsiConsole.MarkupLine($"[bold red]✗ No local configuration found at {StoreRoot.FullName}[/]");
            return null;
        }
        var sync = new AppApiSynchronizer(localStore)
        {
        };
        return sync;
    }

    public async Task<int> RunAsync(CliContext context, StatusContext console)
    {
        var sync = Initialize();
        if (sync is null)
        {
            return ExitCode.BadRequest;
        }
        var selector = new SyncItemSelector { Filename = Filename, Name = AppApiMapper.ToSafeName(Name), Namespace = Namespace, };

        if (SynchronizationTarget == SynchronizationTarget.PocketID)
        {
            console.Status("Loading local specifications ...");
            var loadLocalResponse = await sync.LoadConfiguration(selector, context.CancellationToken);
            if (Verbose)
            {
                foreach (var api in sync.Items)
                {
                    AnsiConsole.MarkupLine($"[green]✓ Local specification for [bold]application API {api.Namespace}/{api.Name}[/] read from {Path.GetRelativePath(StoreRoot.FullName, api.Filename!)}[/]");
                    AnsiConsole.WriteLine(Yaml.Write(api.Local));
                }
            }
            if (loadLocalResponse != ExitCode.Success || sync.Items.Count == 0)
            {
                AnsiConsole.MarkupLine($"[red]✗ Invalid content in {StoreRoot.FullName}[/]");
                return loadLocalResponse;
            }
        }

        console.Status("Loading Pocket ID specifications ...");

        var pocketId = HttpClientFactory.Connect(PocketIdUri, ApiKey);
        var version = await pocketId.Version.GetAsync(context.CancellationToken);
        if (version.IsSuccessful && version.Data is not null)
        {
            AnsiConsole.MarkupLine($"[Gray]Pocket ID version [bold]{version.Data.CurrentVersion}[/] at {version.Uri}[/]");
            AnsiConsole.MarkupLine($"[Gray]Using local storage at {StoreRoot.FullName}[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]{version.Status}[/]; GET [bold]{version.Uri}[/]");
            return ExitCode.FatalError;
        }

        var appApiResult = await sync.CombineAsync(pocketId, SynchronizationTarget, selector, context.CancellationToken);
        if (appApiResult.ExitCode != ExitCode.Success)
        {
            switch (appApiResult.ExitCode)
            {
                case ExitCode.FatalError:
                    AnsiConsole.MarkupLine($"[red]✗ Pocket ID application API [bold]{appApiResult.Client?.Id}[/] failed to read: {appApiResult.ErrorMessage}[/]");
                    break;

                case ExitCode.BadRequest:
                    AnsiConsole.MarkupLine($"[red]✗ Pocket ID application API [bold]{appApiResult.Client?.Id}[/] is invalid: {appApiResult.ErrorMessage}[/]");
                    break;

                default:
                    AnsiConsole.MarkupLine($"[red]✗ Pocket ID application API [bold]{appApiResult.Client?.Id}[/] failed: {appApiResult.ErrorMessage}[/]");
                    break;
            }
            return appApiResult.ExitCode;
        }
        if (Verbose)
        {
            AnsiConsole.MarkupLine($"Pocket ID {sync.Items.Count} application API(s) loaded");
        }

        if (SynchronizationTarget == SynchronizationTarget.Configuration)
        {
            console.Status("Loading local specifications ...");

            var mergeLocalResponse = await sync.LoadConfiguration(null, context.CancellationToken);
            if (mergeLocalResponse != ExitCode.Success)
            {
                return mergeLocalResponse;
            }
        }

        ShowApplicationApis(sync.Items);

        if (DryRun)
        {
            AnsiConsole.MarkupLine($"[blue bold]ⓘ Nothing has been changed, dry run is enabled[/]");
            return ExitCode.Success;
        }

        console.Status("Syncing Pocket ID to local specifications ...");
        var syncResult = await sync.SynchronizeAsync(pocketId, SynchronizationTarget, context.CancellationToken);
        AnsiConsole.MarkupLine("[bold]Synchronization results:[/]");
        ShowSyncResults(sync.Items);
        AnsiConsole.MarkupLine("[bold]Synchronization completed[/]");

        if (syncResult != ExitCode.Success)
        {
            AnsiConsole.MarkupLine("[red bold]✗ Synchronization was not fully successfull, see details above[/]");
        }
        return syncResult;
    }

    private void ShowSyncResults(List<AppApiSyncItem> apis)
    {
        foreach (var api in apis.Where(c => c.IsRemoteEqualLocal == false || c.IsLocalDirty == true).OrderBy(c => c.Id, StringComparer.OrdinalIgnoreCase))
        {
            if (SynchronizationTarget == SynchronizationTarget.PocketID)
            {
                if (api.RemoteMerged is not null && api.HasError == false)
                {
                    AnsiConsole.MarkupLine($"[green1]✓ Pocket ID application API [bold]{api.Name}[/] id({api.Id!}) synchronized[/]");
                    if (Verbose)
                    {
                        JsonHelper.WriteConsole(api.RemoteMerged);
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine($"✗[red] Pocket ID application API [bold]{api.Name}[/] id({api.Id!}) failed to {(api.Remote is not null ? "update" : "create")}: {Markup.Escape(api.Message ?? "")}[/]");
                    if (Verbose)
                    {
                        AnsiConsole.Markup($"# [bold]{Path.GetRelativePath(StoreRoot.FullName, api.Filename!)}[/]\n{Markup.Escape(Yaml.Write(api.Local))}\n");
                    }
                }
            }
            if (SynchronizationTarget == SynchronizationTarget.Configuration)
            {
                if (api.LocalMerged is not null && api.HasError == false)
                {
                    AnsiConsole.MarkupLine($"[green1]✓ Pocket ID application API [bold]{api.Name}[/] id({api.Id!}) {(api.Local is not null ? "updated" : "created")} specification at {Path.GetRelativePath(StoreRoot.FullName, api.Filename!)}[/]");
                    if (Verbose)
                    {
                        JsonHelper.WriteConsole(api.RemoteMerged);
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]✗ Pocket ID application API [bold]{api.Name}[/] id({api.Id!}) failed to {(api.Local is not null ? "update" : "create")} specification at {Path.GetRelativePath(StoreRoot.FullName, api.Filename!)}: {Markup.Escape(api.Message ?? "")}[/]");
                    if (Verbose)
                    {
                        AnsiConsole.Markup($"# [bold]{Path.GetRelativePath(StoreRoot.FullName, api.Filename!)}[/]\n{Markup.Escape(Yaml.Write(api.Local))}\n");
                    }
                }
            }
        }
    }

    private void ShowApplicationApis(List<AppApiSyncItem> apis)
    {
        foreach (var api in apis.OrderBy(c => c.Id, StringComparer.OrdinalIgnoreCase))
        {
            if (api.HasError == true)
            {
                AnsiConsole.MarkupLine($"[red]✗ Pocket ID application API [bold]{api.Name}[/] id({api.Id!}) has errors[/]");

            }
            else if (api.Remote is null)
            {
                AnsiConsole.MarkupLine($"[Orange1]✗ Pocket ID application API [bold]{api.Name}[/] id({api.Id!}) doesn't exist[/]");
            }
            else
            {
                if (api.IsRemoteEqualLocal)
                {
                    if (api.IsLocalDirty)
                    {
                        AnsiConsole.MarkupLine($"[Orange3]✓ Pocket ID application API [bold]{api.Name}[/] id({api.Id!}) is unchanged (sync forced)[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[green]✓ Pocket ID application API [bold]{api.Name}[/] id({api.Id!}) is unchanged[/]");
                    }
                }
                else
                {
                    if (api.Local is not null)
                    {
                        AnsiConsole.MarkupLine($"[Orange3]✗ Pocket ID application API [bold]{api.Name}[/] id({api.Id!}) is changed[/]");
                        if (Verbose)
                        {
                            foreach (var diff in AppApiSpec.EqualityComparer.Default.Inequalities(api.Local.Spec, api.Remote.ToKind(Namespace).Spec))
                            {
                                AnsiConsole.MarkupLine($" - Difference: [Orange3]{AnsiMarkup.Escape(diff.ToString())}[/]");
                            }
                        }
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[Orange1]✗ Pocket ID application API [bold]{api.Name}[/] id({api.Id!}) doesn't exist[/]");
                    }
                    // AnsiConsole.MarkupLine($"L{client.Local is not null} - {client.LocalMerged is not null}, R{client.Remote is not null} - {client.RemoteMerged is not null}, {client.IsLocalDirty}:{client.IsRemoteEqualLocal}");
                    if (Verbose)
                    {
                        if (api.Local is not null)
                        {
                            AnsiConsole.MarkupLine($"[gray bold]Local:[/]\n{Yaml.Write(api.Local)}\n");
                        }
                        if (api.Remote is not null)
                        {
                            AnsiConsole.MarkupLine($"[gray bold]Remote[/]:\n{Yaml.Write(api.Remote.ToKind(Namespace))}");
                        }
                    }
                }
            }
        }
    }
}
