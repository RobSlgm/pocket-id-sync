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
    public RootCommand? RootCommand { get; set; }

    public Task<int> RunAsync(CliContext context)
    {
        context.ShowHelp();
        return Task.FromResult(ExitCode.Success);
    }
}
