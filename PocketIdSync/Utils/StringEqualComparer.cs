using System;
using System.Collections.Generic;

namespace PocketIdSync.Utils;

public sealed class StringEmptyEqualityComparer : IEqualityComparer<string?>
{
    public static readonly StringEmptyEqualityComparer Default = new();

    public bool Equals(string? x, string? y)
    {
        var normalizedX = string.IsNullOrEmpty(x) ? string.Empty : x;
        var normalizedY = string.IsNullOrEmpty(y) ? string.Empty : y;

        return string.Equals(normalizedX, normalizedY, StringComparison.Ordinal);
    }

    public int GetHashCode(string? obj)
    {
        if (string.IsNullOrEmpty(obj))
        {
            return string.Empty.GetHashCode(StringComparison.Ordinal);
        }

        return obj.GetHashCode(StringComparison.Ordinal);
    }
}
