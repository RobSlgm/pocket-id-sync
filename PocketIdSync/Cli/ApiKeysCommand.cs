using System.Threading.Tasks;
using DotMake.CommandLine;

namespace PocketIdSync.Cli;

[CliCommand(
    Description = "Pocket API Key management",
    ShortFormAutoGenerate = CliNameAutoGenerate.Options,
    Parent = typeof(RootCommand)
)]
sealed class ApiKeysCommand
{
    public RootCommand? RootCommand { get; set; }

    public Task<int> RunAsync(CliContext context)
    {
        context.ShowHelp();
        return Task.FromResult(ExitCode.Success);
    }
}
