using PocketIdSync.Models;
using PocketIdSync.ModelSpecs;

namespace PocketIdSync.Sync;

sealed class OidcClientSyncItem : ISyncItem<OidcClientKind, OidcClientCompleteDto>
{
    public string? Filename { get; set; }
    public string? Namespace { get; set; }
    public string? Name { get; set; }
    public string? Id { get; set; }
    public string? Secret { get; set; }

    public OidcClientKind? Local
    {
        get;
        set
        {
            field = value;
            Verify();
        }
    }
    public OidcClientKind? LocalMerged { get; set; }

    public OidcClientCompleteDto? Remote
    {
        get;
        set
        {
            field = value;
            Verify();
        }
    }
    public OidcClientCompleteDto? RemoteMerged { get; set; }
    public bool IsRemoteEqualLocal { get; private set; }

    private bool Verify()
    {
        if (Remote is null || Local is null)
        {
            IsRemoteEqualLocal = false;
            return IsRemoteEqualLocal;
        }
        var remoteSpec = Remote.ToKind(Local, null);// TODO: AppApiResolver
        IsRemoteEqualLocal = Local.Spec == remoteSpec.Spec;
        return IsRemoteEqualLocal;
    }

    public bool HasError { get; private set; }
    public string? Message { get; private set; }
    public bool IsLocalDirty { get; set; }

    public void SetError(string? message = null)
    {
        if (HasError == false)
        {
            HasError = true;
            Message = message;
        }
    }
}

internal static class OidcClientSyncItemExtensions
{
    extension(OidcClientSyncItem Item)
    {
        public bool IsMatch(SyncItemSelector? selector)
        {
            if (selector is null) return true;
            if (!string.IsNullOrEmpty(selector.Id) && !string.IsNullOrEmpty(Item.Id))
            {
                return string.Equals(selector.Id, Item.Id, System.StringComparison.OrdinalIgnoreCase);
            }
            if (!string.IsNullOrEmpty(selector.Name) && !string.IsNullOrEmpty(Item.Name))
            {
                return string.Equals(selector.Name, Item.Name, System.StringComparison.OrdinalIgnoreCase);
            }
            if (!string.IsNullOrEmpty(selector.Filename) && !string.IsNullOrEmpty(Item.Filename))
            {
                return string.Equals(selector.Filename, Item.Filename, System.StringComparison.OrdinalIgnoreCase);
            }
            if (!string.IsNullOrEmpty(selector.Namespace) && !string.IsNullOrEmpty(Item.Namespace))
            {
                return string.Equals(selector.Namespace, Item.Namespace, System.StringComparison.OrdinalIgnoreCase);
            }
            return true;
        }
    }
}
