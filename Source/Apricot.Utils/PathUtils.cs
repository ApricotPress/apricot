namespace Apricot.Utils;

public static class PathUtils
{
    /// <summary>
    /// Normalize path to system agnostic path with / as path separator. Also trims slashes at end. 
    /// </summary>
    public static string NormalizePath(string path) => path.Replace("\\", "/").TrimEnd('/');

    /// <summary>
    /// Gets normalized path of a parent directory. 
    /// </summary>
    public static string GetParent(string path) => NormalizePath(Path.GetDirectoryName(path)!);
}
