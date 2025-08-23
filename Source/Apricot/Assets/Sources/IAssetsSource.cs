namespace Apricot.Assets.Sources;

/// <summary>
/// Abstraction over file system or any other place where assets can be stored (e.g. embedded into dll). 
/// </summary>
public interface IAssetsSource
{
    string Scheme { get; }

    IEnumerable<string> ListAssetsPaths(string localPath, ListAssetsType listType);

    Stream Open(string localPath);
}
