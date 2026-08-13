using System.Threading.Tasks;
using DotMake.CommandLine;

namespace PocketIdSync.Cli.Server;

[CliCommand(
    Description = "Manage API keys to access Pocket ID",
    ShortFormAutoGenerate = CliNameAutoGenerate.Options,
    Parent = typeof(ServerCommand)
)]
sealed class ApiKeyCommand
{
#pragma warning disable CA1822 // Mark members as static
    public Task<int> RunAsync(CliContext context)
#pragma warning restore CA1822 // Mark members as static
    {
        context.ShowHelp();
        return Task.FromResult(ExitCode.Success);
    }
}
