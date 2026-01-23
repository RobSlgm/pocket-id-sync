namespace PocketIdSync.Sync;

public record ConfigStoreFileResult(int ExitCode, byte[]? Content, string? Mimetype, string? Filename)
{
    public bool IsSuccessful
    {
        get
        {
            return ExitCode == PocketIdSync.ExitCode.Success;
        }
    }
}
