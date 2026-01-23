using System.IO;
using DotMake.CommandLine;
using PocketIdSync.Sync;

namespace PocketIdSync.Cli;

internal abstract class SyncCommandBase : AuthorizationCommandBase
{
    [CliOption(Description = "Filename (selector)", Alias = "f", Required = false)]
    public string? Filename { get; set; }

    [CliOption(Description = "Name of specification (selector)", Alias = "n", Required = false)]
    public string? Name { get; set; }

    [CliOption(Description = "Namespace", Alias = "ns", Hidden = true)]
    public string Namespace { get; set; } = "default";

    [CliOption(Description = "Root directory of file store for configuration", Alias = "d", ValidationRules = CliValidationRules.ExistingDirectory)]
    public DirectoryInfo StoreRoot { get; set; } = new DirectoryInfo(".");

    [CliOption(Description = "Synchronization target", Name = "synchronize", Alias = "s", Required = false)]
    public SynchronizationTarget SynchronizationTarget { get; set; } = SynchronizationTarget.PocketID;

    [CliOption(Description = "Verbose", Alias = "v")]
    public bool Verbose { get; set; }

    [CliOption(Description = "Dry run")]
    public bool DryRun { get; set; }
}
