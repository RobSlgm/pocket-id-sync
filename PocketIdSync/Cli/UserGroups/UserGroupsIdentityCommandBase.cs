using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotMake.CommandLine;
using PocketIdSync.Apis;
using Spectre.Console;

namespace PocketIdSync.Cli.UserGroups;

abstract class UserGroupsIdentityCommandBase : AuthorizationCommandBase
{
    [CliArgument(Description = "User group Id", Required = false, Arity = CliArgumentArity.ZeroOrOne)]
    public string? UserGroupId { get; set; }

    [CliOption(Description = "Name", Required = false)]
    public string? Name { get; set; }


    public static async Task<(int ExitCode, string? Id)> FindUserGroup(PocketIdClient pocketId, string? UserGroupId, string? Name, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(UserGroupId))
        {
            if (string.IsNullOrEmpty(Name))
            {
                AnsiConsole.MarkupLine($"[red]✗ Either the user group Id or name is required[/]");
                return (ExitCode.BadRequest, null);
            }
            var usergroups = await pocketId.UserGroups.ListAsync(ct);
            if (!usergroups.IsSuccessful)
            {
                AnsiConsole.MarkupLine($"[red]✗ Pocket ID UserGroup search failed[/]");
                return (ExitCode.FatalError, null);
            }
            UserGroupId = usergroups.Data?.FirstOrDefault(u => string.Equals(u.Name, Name, System.StringComparison.OrdinalIgnoreCase))?.Id;
            if (UserGroupId is null)
            {
                AnsiConsole.MarkupLine($"[red]✗ Pocket ID UserGroup with Name=[bold]{Name}[/] not found[/]");
                return (ExitCode.BadRequest, null);
            }
        }
        return (ExitCode.Success, UserGroupId);
    }
}
