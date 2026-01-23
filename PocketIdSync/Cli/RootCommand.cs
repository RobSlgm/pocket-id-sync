using System.Threading.Tasks;
using DotMake.CommandLine;

namespace PocketIdSync.Cli;

[CliCommand(Description = "Pocket ID Sync")]
sealed class RootCommand
{
#pragma warning disable CA1822 // Mark members as static
    public async Task RunAsync(CliContext context)
#pragma warning restore CA1822 // Mark members as static
    {
        if (!context.Result.HasArgs)
            context.ShowHelp();
        else
            context.ShowValues();
        await Task.Delay(0, context.CancellationToken);
    }

}
