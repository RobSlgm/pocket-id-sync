using System.Threading.Tasks;
using DotMake.CommandLine;

namespace PocketIdSync.Cli.Server;

[CliCommand(
    Description = "Read and write Pocket ID server instance images",
    ShortFormAutoGenerate = CliNameAutoGenerate.Options,
    Name = "image",
    Parent = typeof(ServerCommand)
)]
sealed class ServerImagesCommand
{
#pragma warning disable CA1822 // Mark members as static
    public Task<int> RunAsync(CliContext context)
#pragma warning restore CA1822 // Mark members as static
    {
        context.ShowHelp();
        return Task.FromResult(ExitCode.Success);
    }
}
