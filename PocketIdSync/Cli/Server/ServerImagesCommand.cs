using System.Threading.Tasks;
using DotMake.CommandLine;

namespace PocketIdSync.Cli.Server;

[CliCommand(
    Description = "Manage Pocket ID server instance splash graphic",
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
