namespace PocketIdSync.Sync;

record ConfigStoreFile(byte[]? Content = default, string? Mimetype = default, string? Filename = default, bool isSidecar = false, int ExitCode = ExitCode.BadRequest)
{
    public bool IsSuccessful
    {
        get
        {
            return ExitCode == PocketIdSync.ExitCode.Success;
        }
    }
}
