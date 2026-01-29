namespace PocketIdSync.Sync;

public record ConfigStoreFileResult(int ExitCode, byte[]? Content, string? Mimetype, string? Filename, bool isSidecar)
{
    public bool IsSuccessful
    {
        get
        {
            return ExitCode == PocketIdSync.ExitCode.Success;
        }
    }
}
