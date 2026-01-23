using System.Text.Json.Serialization;

namespace PocketIdSync.Sync;

[JsonConverter(typeof(SynchronizationTarget))]
public enum SynchronizationTarget
{
    PocketID,
    Configuration,
}
