using System.Threading.Tasks;
using DotMake.CommandLine;

namespace PocketIdSync.Cli;

[CliCommand(
    Description = "Read and write application configuration",
    ShortFormAutoGenerate = CliNameAutoGenerate.Options,
    Parent = typeof(RootCommand)
)]
sealed class ApplicationConfigurationCommand
{
    public RootCommand? RootCommand { get; set; }

    public Task<int> RunAsync(CliContext context)
    {
        context.ShowHelp();
        return Task.FromResult(ExitCode.Success);
    }
}
