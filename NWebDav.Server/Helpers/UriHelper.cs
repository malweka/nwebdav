using System;

namespace NWebDav.Server.Helpers;

public static class UriHelper
{
    public static Uri Combine(Uri baseUri, string path)
    {
        var uriText = baseUri.OriginalString;
        if (uriText.EndsWith("/"))
            uriText = uriText[..^1];
        return new Uri($"{uriText}/{path}", UriKind.Absolute);
    }

    public static string ToEncodedString(Uri entryUri)
    {
        return entryUri
            .AbsoluteUri
            .Replace("#", "%23")
            .Replace("[", "%5B")
            .Replace("]", "%5D");
    }

    public static string GetDecodedPath(Uri uri)
    {
        return uri.LocalPath + Uri.UnescapeDataString(uri.Fragment);
    }

    public static Uri RemovePrefix(Uri uri, string davPrefix)
    {
        if (uri == null) throw new ArgumentNullException(nameof(uri));
        if (string.IsNullOrWhiteSpace(davPrefix)) return uri;

        string prefixWithSlash = "/" + davPrefix.Trim('/');
        string path = uri.AbsolutePath;

        if (path.StartsWith(prefixWithSlash, StringComparison.OrdinalIgnoreCase))
        {
            path = path.Substring(prefixWithSlash.Length);
        }

        string newUri = uri.GetLeftPart(UriPartial.Authority) + path;
        return new Uri(newUri);
    }
}
