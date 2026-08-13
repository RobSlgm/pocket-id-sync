using System.Text.RegularExpressions;

namespace PocketIdSync.Utils;


static partial class StringNameConverter
{
    public static string ToSafeName(string? uri)
    {
        if (string.IsNullOrEmpty(uri))
        {
            return string.Empty;
        }
        var lowerInput = uri.ToLowerInvariant();
        return AllowedNameCharacterset.Replace(lowerInput, "-");
    }


    [GeneratedRegex("[^a-z0-9.\\-]+", RegexOptions.None, matchTimeoutMilliseconds: 100)]
    private static partial Regex AllowedNameCharacterset { get; }
}
