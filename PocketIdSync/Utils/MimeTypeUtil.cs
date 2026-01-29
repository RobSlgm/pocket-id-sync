using System;
using System.Collections.Immutable;
using System.Linq;
namespace PocketIdSync.Utils;


static class MimeTypeUtil
{
    /// <summary>
    /// Convert common file extension to a mime type (e.g. .jpg to image/jpeg)
    /// </summary>
    /// <param name="extension">Extension with or without leading dot</param>
    /// <returns>Mimetype or null</returns>
    public static string? ToMimeType(string? extension)
    {
        if (extension is null) return null;
        if (!extension.StartsWith('.'))
        {
            extension = $".{extension}";
        }
        var (_, mimetype) = Map.FirstOrDefault(m => string.Equals(m.Extension, extension, StringComparison.OrdinalIgnoreCase));
        return mimetype;
    }

    /// <summary>
    /// Convert mime type to a common file extension (e.g. image/jpg to .jpg)
    /// </summary>
    /// <param name="mimetype"></param>
    /// <returns>Extension (with leading dot) or null</returns>
    public static string? FromMimeType(string? mimetype)
    {
        if (mimetype is null) return null;
        var (extension, _) = Map.FirstOrDefault(m => string.Equals(m.MimeType, mimetype, StringComparison.OrdinalIgnoreCase));
        return extension;
    }

    private static readonly ImmutableArray<(string Extension, string MimeType)> Map =
    [
        (".gif", "image/gif"),
        (".ico", "image/x-icon"),
        (".ico", "image/vnd.microsoft.icon"),
        (".jpg", "image/jpeg"),
        (".jpe", "image/jpeg"),
        (".jpeg", "image/jpeg"),
        (".png", "image/png"),
        (".svg", "image/svg+xml"),
        (".webp", "image/webp"),
        (".avif", "image/avif"),
        (".heic", "image/heic"),
        (".heic", "image/heif"),
    ];
}
