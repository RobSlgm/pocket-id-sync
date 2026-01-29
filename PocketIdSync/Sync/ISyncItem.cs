namespace PocketIdSync.Sync;

interface ISyncItem<L, R>
{
    string? Filename { get; set; }
    string? Namespace { get; set; }
    string? Name { get; set; }
    string? Id { get; set; }
    L? Local { get; set; }
    L? LocalMerged { get; set; }
    R? Remote { get; set; }
    R? RemoteMerged { get; set; }
    bool IsRemoteEqualLocal { get; }

    void SetError(string? message = null);
    bool HasError { get; }
    string? Message { get; }
    bool IsLocalDirty { get; set; }
}
