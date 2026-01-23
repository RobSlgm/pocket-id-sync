using System.Threading.Tasks;
using DotMake.CommandLine;
using PocketIdSync.Apis;
using PocketIdSync.ModelSpecs;
using PocketIdSync.Utils;
using Spectre.Console;

namespace PocketIdSync.Cli.UserGroups;


[CliCommand(
    Description = "Get user group configuration from Pocket ID",
    Name = "get",
    Parent = typeof(UserGroupsCommand)
)]
sealed class GetCommand : UserGroupsIdentityCommandBase
{
    private readonly JsonHelper JsonHelper;
    private readonly YamlHelper Yaml;

    public GetCommand(JsonHelper jsonHelper, YamlHelper yamlHelper)
    {
        JsonHelper = jsonHelper;
        Yaml = yamlHelper;
    }

    [CliOption(Description = "Namespace, used in YAML generation", Alias = "ns", Hidden = true)]
    public string Namespace { get; set; } = "default";

    [CliOption(Description = "Output format", Alias = "o", AllowedValues = ["json", "yaml", "console"], Arity = CliArgumentArity.ZeroOrOne)]
    public string Output { get; set; } = "console";

    public async Task<int> RunAsync(CliContext context)
    {
        var pocketId = new PocketIdClient(PocketIdUri, ApiKey);
        var findUserGroup = await FindUserGroup(pocketId, UserGroupId, Name, context.CancellationToken);
        if (findUserGroup.ExitCode != ExitCode.Success || string.IsNullOrEmpty(findUserGroup.Id))
        {
            return findUserGroup.ExitCode;
        }
        UserGroupId = findUserGroup.Id;
        var client = await pocketId.UserGroups.Id(UserGroupId).GetAsync(context.CancellationToken);
        if (!client.IsSuccessful)
        {
            AnsiConsole.MarkupLine($"[red]✗ Pocket ID call {client.Uri} failed[/]");
            return ExitCode.BadRequest;
        }
        if (client.Data is null)
        {
            AnsiConsole.MarkupLine($"[red]✗ Pocket ID UserGroup [bold]{UserGroupId}[/] not found[/]");
            return ExitCode.BadRequest;
        }
        switch (Output)
        {
            case "json":
                JsonHelper.WriteConsole(client.Data);
                break;
            case "yaml":
                AnsiConsole.WriteLine(Yaml.Write(client.Data.ToKind(Namespace)));
                break;
            default:
                JsonHelper.WriteConsole(client.Data);
                AnsiConsole.WriteLine(Yaml.Write(client.Data.ToKind(Namespace)));
                break;
        }
        return ExitCode.Success;
    }
}
