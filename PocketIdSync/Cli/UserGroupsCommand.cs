using System.Threading.Tasks;
using DotMake.CommandLine;

namespace PocketIdSync.Cli;

[CliCommand(
    Description = "Synchronize user groups",
    ShortFormAutoGenerate = CliNameAutoGenerate.Options,
    Parent = typeof(RootCommand)
)]
sealed class UserGroupsCommand
{
    public RootCommand? RootCommand { get; set; }

    public Task<int> RunAsync(CliContext context)
    {
        context.ShowHelp();
        return Task.FromResult(ExitCode.Success);
    }
}
