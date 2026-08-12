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

namespace PocketIdSync.Cli.OidcClient;

[CliCommand(
    Description = "Sync OIDC client configuration",
    Name = "sync",
    ShortFormAutoGenerate = CliNameAutoGenerate.Options,
    Parent = typeof(OidcClientCommand)
)]
sealed class SyncCommand(JsonHelper JsonHelper, YamlHelper Yaml, IHttpClientFactory HttpClientFactory) : SyncCommandBase
{
    [CliOption(Description = "Id (selector)", Alias = "", Required = false)]
    public string? Id { get; set; }

    [CliOption(Description = "Force synchronization of logos", Alias = "logo", Required = false)]
    public bool ForceLogoSynchronization { get; set; }

    [CliOption(Description = "Store logos outside the YAML configuration. Needs to be supported by the used configuration store", Required = false)]
    public bool SidecarLogo { get; set; }

    public async Task<int> RunAsync(CliContext context)
    {
        var exitCode = await AnsiConsole.Status()
            .Spinner(Spinner.Known.BluePulse)
            .StartAsync("Sync OIDC client configuration ...", async ctx => await RunAsync(context, ctx));
        return exitCode;
    }

    private OidcClientSynchronizer? Initialize()
    {
        var localStore = new OidcClientFilestore(StoreRoot.FullName, Yaml)
        {
            UseSidecars = SidecarLogo,
        };
        if (!localStore.Exists)
        {
            AnsiConsole.MarkupLine($"[bold red]✗ No local configuration found at {StoreRoot.FullName}[/]");
            return null;
        }
        var sync = new OidcClientSynchronizer(localStore)
        {
            ForceLogoSynchronization = ForceLogoSynchronization,
        };
        localStore.Resolver = sync.AppApiResolver;
        return sync;
    }

    public async Task<int> RunAsync(CliContext context, StatusContext console)
    {
        var sync = Initialize();
        if (sync is null)
        {
            return ExitCode.BadRequest;
        }
        var selector = new SyncItemSelector { Filename = Filename, Name = Name, Namespace = Namespace, Id = Id, };
        if (SynchronizationTarget == SynchronizationTarget.PocketID)
        {
            console.Status("Loading local specifications ...");
            var loadLocalResponse = await sync.LoadConfiguration(selector, context.CancellationToken);
            if (Verbose)
            {
                foreach (var client in sync.Items)
                {
                    AnsiConsole.MarkupLine($"[green]✓ Local specification for [bold]OidcClient {client.Namespace}/{client.Name}[/] read from {Path.GetRelativePath(StoreRoot.FullName, client.Filename!)}[/]");
                    AnsiConsole.WriteLine(Yaml.Write(client.Local));
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

        var oidcClientResult = await sync.CombineAsync(pocketId, SynchronizationTarget, selector, context.CancellationToken);
        if (oidcClientResult.ExitCode != ExitCode.Success)
        {
            switch (oidcClientResult.ExitCode)
            {
                case ExitCode.FatalError:
                    AnsiConsole.MarkupLine($"[red]✗ Pocket ID OidcClient [bold]{oidcClientResult.Client?.Id}[/] failed to read: {oidcClientResult.ErrorMessage}[/]");
                    break;

                case ExitCode.BadRequest:
                    AnsiConsole.MarkupLine($"[red]✗ Pocket ID OidcClient [bold]{oidcClientResult.Client?.Id}[/] is invalid: {oidcClientResult.ErrorMessage}[/]");
                    break;

                default:
                    AnsiConsole.MarkupLine($"[red]✗ Pocket ID OidcClient [bold]{oidcClientResult.Client?.Id}[/] failed: {oidcClientResult.ErrorMessage}[/]");
                    break;
            }
            return oidcClientResult.ExitCode;
        }
        if (Verbose)
        {
            AnsiConsole.MarkupLine($"Pocket ID {sync.Items.Count} OidcClient(s) loaded");
        }

        if (SynchronizationTarget == SynchronizationTarget.Configuration)
        {
            console.Status("Loading local specifications ...");

            var mergeLocalResponse = await sync.LoadConfiguration(default, context.CancellationToken);
            if (mergeLocalResponse != ExitCode.Success)
            {
                return mergeLocalResponse;
            }
        }

        ShowClients(sync.Items);

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

    private void ShowSyncResults(List<OidcClientSyncItem> clients)
    {
        foreach (var client in clients.Where(c => c.IsRemoteEqualLocal == false || c.IsLocalDirty == true).OrderBy(c => c.Id, StringComparer.OrdinalIgnoreCase))
        {
            if (SynchronizationTarget == SynchronizationTarget.PocketID)
            {
                if (client.RemoteMerged is not null && client.HasError == false)
                {
                    AnsiConsole.MarkupLine($"[green1]✓ Pocket ID OidcClient [bold]{client.Name}[/] id({client.Id!}) synchronized[/]");
                    if (Verbose)
                    {
                        JsonHelper.WriteConsole(client.RemoteMerged);
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine($"✗[red] Pocket ID OidcClient [bold]{client.Name}[/] id({client.Id!}) failed to {(client.Remote is not null ? "update" : "create")}: {Markup.Escape(client.Message ?? "")}[/]");
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
                    AnsiConsole.MarkupLine($"[green1]✓ Pocket ID OidcClient [bold]{client.Name}[/] id({client.Id!}) {(client.Local is not null ? "updated" : "created")} specification at {Path.GetRelativePath(StoreRoot.FullName, client.Filename!)}[/]");
                    if (Verbose)
                    {
                        JsonHelper.WriteConsole(client.RemoteMerged);
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]✗ Pocket ID OidcClient [bold]{client.Name}[/] id({client.Id!}) failed to {(client.Local is not null ? "update" : "create")} specification at {Path.GetRelativePath(StoreRoot.FullName, client.Filename!)}: {Markup.Escape(client.Message ?? "")}[/]");
                    if (Verbose)
                    {
                        AnsiConsole.Markup($"# {Path.GetRelativePath(StoreRoot.FullName, client.Filename!)}[/]\n{Yaml.Write(client.Local)}\n");
                    }
                }
            }
            if (!string.IsNullOrEmpty(client.Secret))
            {
                AnsiConsole.MarkupLine($"[Gold1]Secret [bold]{client.Secret}[/] created, store safely[/]");
            }
        }
    }

    private void ShowClients(List<OidcClientSyncItem> clients)
    {
        foreach (var client in clients.OrderBy(c => c.Id, StringComparer.OrdinalIgnoreCase))
        {
            if (client.HasError == true)
            {
                AnsiConsole.MarkupLine($"[red]✗ Pocket ID OidcClient [bold]{client.Name}[/] id({client.Id!}) has errors[/]");

            }
            else if (client.Remote is null)
            {
                AnsiConsole.MarkupLine($"[Orange1]✗ Pocket ID OidcClient [bold]{client.Name}[/] id({client.Id!}) doesn't exist[/]");
            }
            else
            {
                if (client.IsRemoteEqualLocal)
                {
                    if (client.IsLocalDirty)
                    {
                        AnsiConsole.MarkupLine($"[Orange3]✓ Pocket ID OidcClient [bold]{client.Name}[/] id({client.Id!}) is unchanged (sync forced)[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[green]✓ Pocket ID OidcClient [bold]{client.Name}[/] id({client.Id!}) is unchanged[/]");
                    }
                }
                else
                {
                    if (client.Local is not null)
                    {
                        AnsiConsole.MarkupLine($"[Orange3]✗ Pocket ID OidcClient [bold]{client.Name}[/] id({client.Id!}) is changed[/]");
                        if (Verbose)
                        {
                            foreach (var diff in OidcClientSpec.EqualityComparer.Default.Inequalities(client.Local.Spec, client.Remote.ToKind(Namespace).Spec))
                            {
                                AnsiConsole.MarkupLine($" - Difference: [Orange3]{AnsiMarkup.Escape(diff.ToString())}[/]");
                            }
                        }
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[Orange1]✗ Pocket ID OidcClient [bold]{client.Name}[/] id({client.Id!}) doesn't exist[/]");
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
                            // AnsiConsole.MarkupLine($"[gray bold]Remote[/]:\n");
                            // JsonHelper.WriteConsole(client.Remote);
                        }
                    }
                }
            }
        }
    }
}
