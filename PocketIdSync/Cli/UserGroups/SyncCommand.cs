using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotMake.CommandLine;
using PocketIdSync.Apis;
using PocketIdSync.ModelSpecs;
using PocketIdSync.Sync;
using PocketIdSync.Utils;
using Spectre.Console;

namespace PocketIdSync.Cli.UserGroups;


[CliCommand(
    Description = "Sync user group configuration",
    Name = "sync",
    ShortFormAutoGenerate = CliNameAutoGenerate.Options,
    Parent = typeof(UserGroupsCommand)
)]
sealed class SyncCommand : SyncCommandBase
{
    private readonly JsonHelper JsonHelper;
    private readonly YamlHelper Yaml;

    public SyncCommand(JsonHelper jsonHelper, YamlHelper yamlHelper)
    {
        JsonHelper = jsonHelper;
        Yaml = yamlHelper;
    }

    public async Task<int> RunAsync(CliContext context)
    {
        var exitCode = await AnsiConsole.Status()
            .Spinner(Spinner.Known.BluePulse)
            .StartAsync("Sync user group configuration ...", async ctx => await RunAsync(context, ctx));
        return exitCode;
    }

    private ISynchronizer<UserGroupSyncItem>? Initialize()
    {
        var localStore = new UserGroupFilestore(StoreRoot.FullName, Yaml)
        {
        };
        if (!localStore.Exists)
        {
            AnsiConsole.MarkupLine($"[bold red]✗ No local configuration found at {StoreRoot.FullName}[/]");
            return null;
        }
        var sync = new UserGroupSynchronizer(localStore)
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
        var selector = new SyncItemSelector { Filename = Filename, Name = Name, Namespace = Namespace };

        if (SynchronizationTarget == SynchronizationTarget.PocketID)
        {
            console.Status("Loading local specifications ...");
            var loadLocalResponse = await sync.LoadConfiguration(selector, context.CancellationToken);
            if (Verbose)
            {
                foreach (var group in sync.Items)
                {
                    AnsiConsole.MarkupLine($"[green]✓ Local specification for [bold]UserGroup {group.Namespace}/{group.Name}[/] read from {Path.GetRelativePath(StoreRoot.FullName, group.Filename!)}[/]");
                    AnsiConsole.WriteLine(Yaml.Write(group.Local));
                }
            }
            if (loadLocalResponse != ExitCode.Success || sync.Items.Count == 0)
            {
                AnsiConsole.MarkupLine($"[red]✗ Invalid content in {StoreRoot.FullName}[/]");
                return loadLocalResponse;
            }
        }

        console.Status("Loading Pocket ID specifications ...");

        var pocketId = new PocketIdClient(PocketIdUri, ApiKey);
        var version = await pocketId.Version.GetLatest(context.CancellationToken);
        if (version.IsSuccessful)
        {
            AnsiConsole.MarkupLine($"[Gray]Pocket ID version [bold]{version.Data}[/] at {version.Uri}[/]");
            AnsiConsole.MarkupLine($"[Gray]Using local storage at {StoreRoot.FullName}[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]{version.Status}[/]; GET [bold]{version.Uri}[/]");
            return ExitCode.FatalError;
        }

        var userGroupResult = await sync.CombineAsync(pocketId, SynchronizationTarget, selector, context.CancellationToken);
        if (userGroupResult.ExitCode != ExitCode.Success)
        {
            switch (userGroupResult.ExitCode)
            {
                case ExitCode.FatalError:
                    AnsiConsole.MarkupLine($"[red]✗ Pocket ID UserGroup [bold]{userGroupResult.Client?.Id}[/] failed to read: {userGroupResult.ErrorMessage}[/]");
                    break;

                case ExitCode.BadRequest:
                    AnsiConsole.MarkupLine($"[red]✗ Pocket ID UserGroup [bold]{userGroupResult.Client?.Id}[/] is invalid: {userGroupResult.ErrorMessage}[/]");
                    break;

                default:
                    AnsiConsole.MarkupLine($"[red]✗ Pocket ID UserGroup [bold]{userGroupResult.Client?.Id}[/] failed: {userGroupResult.ErrorMessage}[/]");
                    break;
            }
            return userGroupResult.ExitCode;
        }
        if (Verbose)
        {
            AnsiConsole.MarkupLine($"Pocket ID {sync.Items.Count} UserGroup(s) loaded");
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

        ShowGroups(sync.Items);

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

    private void ShowSyncResults(List<UserGroupSyncItem> clients)
    {
        foreach (var client in clients.Where(c => c.IsRemoteEqualLocal == false || c.IsLocalDirty == true).OrderBy(c => c.Id, StringComparer.OrdinalIgnoreCase))
        {
            if (SynchronizationTarget == SynchronizationTarget.PocketID)
            {
                if (client.RemoteMerged is not null && client.HasError == false)
                {
                    AnsiConsole.MarkupLine($"[green1]✓ Pocket ID UserGroup [bold]{client.Name}[/] id({client.Id!}) synchronized[/]");
                    if (Verbose)
                    {
                        JsonHelper.WriteConsole(client.RemoteMerged);
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine($"✗[red] Pocket ID UserGroup [bold]{client.Name}[/] id({client.Id!}) failed to {(client.Remote is not null ? "update" : "create")}: {Markup.Escape(client.Message ?? "")}[/]");
                    if (Verbose)
                    {
                        AnsiConsole.Markup($"# {Path.GetRelativePath(StoreRoot.FullName, client.Filename!)}[/]\n{Yaml.Write(client.Local)}\n");
                    }
                }
            }
            if (SynchronizationTarget == SynchronizationTarget.Configuration)
            {
                if (client.LocalMerged is not null && client.HasError == false)
                {
                    AnsiConsole.MarkupLine($"[green1]✓ Pocket ID UserGroup [bold]{client.Name}[/] id({client.Id!}) {(client.Local is not null ? "updated" : "created")} specification at {Path.GetRelativePath(StoreRoot.FullName, client.Filename!)}[/]");
                    if (Verbose)
                    {
                        JsonHelper.WriteConsole(client.RemoteMerged);
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]✗ Pocket ID UserGroup [bold]{client.Name}[/] id({client.Id!}) failed to {(client.Local is not null ? "update" : "create")} specification at {Path.GetRelativePath(StoreRoot.FullName, client.Filename!)}: {Markup.Escape(client.Message ?? "")}[/]");
                    if (Verbose)
                    {
                        AnsiConsole.Markup($"# {Path.GetRelativePath(StoreRoot.FullName, client.Filename!)}[/]\n{Yaml.Write(client.Local)}\n");
                    }
                }
            }
        }
    }

    private void ShowGroups(List<UserGroupSyncItem> clients)
    {
        foreach (var client in clients.OrderBy(c => c.Id, StringComparer.OrdinalIgnoreCase))
        {
            if (client.HasError == true)
            {
                AnsiConsole.MarkupLine($"[red]✗ Pocket ID UserGroup [bold]{client.Name}[/] id({client.Id!}) has errors[/]");

            }
            else if (client.Remote is null)
            {
                AnsiConsole.MarkupLine($"[Orange1]✗ Pocket ID UserGroup [bold]{client.Name}[/] id({client.Id!}) doesn't exist[/]");
            }
            else
            {
                if (client.IsRemoteEqualLocal)
                {
                    if (client.IsLocalDirty)
                    {
                        AnsiConsole.MarkupLine($"[Orange3]✓ Pocket ID UserGroup [bold]{client.Name}[/] id({client.Id!}) is unchanged (sync forced)[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[green]✓ Pocket ID UserGroup [bold]{client.Name}[/] id({client.Id!}) is unchanged[/]");
                    }
                }
                else
                {
                    if (client.Local is not null)
                    {
                        AnsiConsole.MarkupLine($"[Orange3]✗ Pocket ID UserGroup [bold]{client.Name}[/] id({client.Id!}) is changed[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[Orange1]✗ Pocket ID UserGroup [bold]{client.Name}[/] id({client.Id!}) doesn't exist[/]");
                    }
                    // AnsiConsole.MarkupLine($"L{client.Local is not null} - {client.LocalMerged is not null}, R{client.Remote is not null} - {client.RemoteMerged is not null}, {client.IsLocalDirty}:{client.IsRemoteEqualLocal}");
                    if (Verbose)
                    {
                        if (client.Local is not null)
                        {
                            AnsiConsole.MarkupLine($"[gray bold]Local:[/]\n{Yaml.Write(client.Local)}\n");
                        }
                        if (client.Remote is not null)
                        {
                            AnsiConsole.MarkupLine($"[gray bold]Remote[/]:\n{Yaml.Write(client.Remote.ToKind(Namespace))}");
                        }
                    }
                }
            }
        }
    }
}
