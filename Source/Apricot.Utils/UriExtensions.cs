namespace Apricot.Utils;

public static class UriUtils
{
    public static Uri NormalizeAssetsUri(Uri uri) =>
        new UriBuilder
        {
            Scheme = uri.Scheme,
            Host = uri.Host,
            Path = uri.AbsolutePath
        }.Uri;
}
