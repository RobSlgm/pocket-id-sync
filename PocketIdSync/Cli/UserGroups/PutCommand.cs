using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using DotMake.CommandLine;
using PocketIdSync.Apis;
using PocketIdSync.Models;
using PocketIdSync.Utils;
using Spectre.Console;

namespace PocketIdSync.Cli.UserGroups;


[CliCommand(
    Description = "Put client configuration (upload)",
    Name = "put",
    Parent = typeof(UserGroupsCommand)
)]
sealed class PutCommand(JsonHelper JsonHelper, IHttpClientFactory HttpClientFactory) : AuthorizationCommandBase
{
    [CliArgument(Description = "User group configuration file (JSON)", Required = true)]
    public required FileInfo Configuration { get; set; }

    public async Task<int> RunAsync(CliContext context)
    {
        if (!Configuration.Exists)
        {
            AnsiConsole.MarkupLine($"[red]User group configuration file `{Configuration.FullName}` not found[/]");
            return ExitCode.BadRequest;
        }
        var data = await JsonHelper.ReadAsync<UserGroupDto>(Configuration);
        if (data is null || string.IsNullOrEmpty(data.Id))
        {
            AnsiConsole.MarkupLine($"[red]User group configuration `{Configuration.FullName}` invalid[/]");
            return ExitCode.BadRequest;

        }
        AnsiConsole.MarkupLine("[green]✓ Local user group configuration read[/]");
        JsonHelper.WriteConsole(data);
        var pocketId = HttpClientFactory.Connect(PocketIdUri, ApiKey);

        var clientSource = await pocketId.UserGroups.Id(data.Id).GetAsync(context.CancellationToken);
        AnsiConsole.MarkupLine("[green]✓ Pocket ID user group configuration read[/]");
        JsonHelper.WriteConsole(clientSource);

        var client = await pocketId.UserGroups.Id(data.Id).PutAsync(data, context.CancellationToken);
        if (client is null)
        {
            AnsiConsole.MarkupLine($"[bold red]✗ Failed to apply to Pocket ID[/]");
            return ExitCode.BadRequest;
        }
        AnsiConsole.MarkupLine("[green]✓ Applied to Pocket ID[/]");
        JsonHelper.WriteConsole(client);
        return ExitCode.Success;
    }
}
