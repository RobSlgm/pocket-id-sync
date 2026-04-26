using System.Threading.Tasks;
using DotMake.CommandLine;

namespace PocketIdSync.Cli;

[CliCommand(
    Description = "Synchronize OIDC client definitions",
    ShortFormAutoGenerate = CliNameAutoGenerate.Options,
    Parent = typeof(RootCommand)
)]
sealed class OidcClientCommand
{
#pragma warning disable CA1822 // Mark members as static
    public Task<int> RunAsync(CliContext context)
#pragma warning restore CA1822 // Mark members as static
    {
        context.ShowHelp();
        return Task.FromResult(ExitCode.Success);
    }
}
