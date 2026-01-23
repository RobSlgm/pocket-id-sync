using System.Threading.Tasks;
using DotMake.CommandLine;
using PocketIdSync.Apis;
using Spectre.Console;

namespace PocketIdSync.Cli.UserGroups;


[CliCommand(
    Description = "Delete user group configuration from Pocket ID",
    Name = "delete",
    Parent = typeof(UserGroupsCommand)
)]
sealed class DeleteCommand : UserGroupsIdentityCommandBase
{
    public async Task<int> RunAsync(CliContext context)
    {
        var pocketId = new PocketIdClient(PocketIdUri, ApiKey);
        var findUserGroup = await FindUserGroup(pocketId, UserGroupId, Name, context.CancellationToken);
        if (findUserGroup.ExitCode != ExitCode.Success || string.IsNullOrEmpty(findUserGroup.Id))
        {
            return findUserGroup.ExitCode;
        }
        UserGroupId = findUserGroup.Id;
        var client = await pocketId.UserGroups.Id(UserGroupId).DeleteAsync(context.CancellationToken);
        if (!client.IsSuccessful)
        {
            if (client.Status != System.Net.HttpStatusCode.NotFound)
            {
                AnsiConsole.MarkupLine($"[red]✗ Pocket ID call {client.Uri} failed: {client.Status}[/]");
                return ExitCode.BadRequest;
            }
            AnsiConsole.MarkupLine($"[Orange1]✓ Pocket ID UserGroup [bold]{UserGroupId}[/] not found, nothing to do[/]");
            return ExitCode.Success;
        }
        AnsiConsole.MarkupLine($"[green]✓ Pocket ID UserGroup [bold]{UserGroupId}[/] deleted[/]");
        return ExitCode.Success;
    }
}
