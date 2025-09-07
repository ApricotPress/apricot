namespace Apricot.Assets.Sources;

/// <summary>
/// Abstraction over file system or any other place where assets can be stored (e.g. embedded into dll). 
/// </summary>
public interface IAssetsSource
{
    /// <summary>
    /// Scheme associated with this source.
    /// </summary>
    string Scheme { get; }

    // todo: add proper callbacks for asset delete

    /// <summary>
    /// Event that is triggered when asset has changed or created. Provides local path to asset as an argument.
    /// </summary>
    event Action<string> OnAssetChange;

    /// <summary>
    /// Listing of all assets under provided path. Path should omit scheme (those, called local).
    /// </summary>
    IEnumerable<string> ListAssetsPaths(string localPath, ListAssetsType listType);

    /// <summary>
    /// Checks whether asset located at <paramref name="localPath"/> exists in assets source.
    /// </summary>
    bool Exists(string localPath);

    /// <summary>
    /// Gets stream for asset located at path. Path should omit scheme (those, called local).  
    /// </summary>
    /// <exception cref="AssetNotFoundException">Thrown if no asset found under that path.</exception>
    Stream Open(string localPath);
}
