namespace Apricot.Utils;

public static class PathUtils
{
    /// <summary>
    /// Normalize path to system agnostic path with / as path separator. Also trims slashes at end. 
    /// </summary>
    public static string NormalizePath(string path) => path.Replace("\\", "/").TrimEnd('/');
    
    public static bool HasExtension(string path, string extension)
    {
        var actualExtension = Path.GetExtension(path);
        var trimmedExtension = extension.StartsWith('.')
            ? extension[1..]
            : extension;

        return actualExtension.Equals(trimmedExtension, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Gets normalized path of a parent directory. 
    /// </summary>
    public static string GetParent(string path) => NormalizePath(Path.GetDirectoryName(path)!);
}
