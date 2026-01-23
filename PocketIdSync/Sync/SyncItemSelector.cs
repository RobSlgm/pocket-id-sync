namespace PocketIdSync.Sync;

public record SyncItemSelector
{
    public string? Filename { get; set; }
    public string? Name { get; set; }
    public string? Namespace { get; set; }
    public string? Id { get; set; }

    public bool IsRestricted
    {
        get
        {
            return
                !string.IsNullOrEmpty(Filename) ||
                !string.IsNullOrEmpty(Name) ||
                !string.IsNullOrEmpty(Namespace) ||
                !string.IsNullOrEmpty(Id)
                ;
        }
    }
}
