namespace PocketIdSync.Sync;

interface ISyncItem<L, R>
{
    public string? Filename { get; set; }
    public string? Namespace { get; set; }
    public string? Name { get; set; }
    public string? Id { get; set; }
    public L? Local { get; set; }
    public L? LocalMerged { get; set; }
    public R? Remote { get; set; }
    public R? RemoteMerged { get; set; }
    public bool IsRemoteEqualLocal { get; }

    public void SetError(string? message = null);
    public bool HasError { get; }
    public string? Message { get; }
    public bool IsLocalDirty { get; set; }
}
