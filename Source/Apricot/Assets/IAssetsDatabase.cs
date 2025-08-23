using Apricot.Assets.Artifacts;
using Apricot.Assets.Importing;
using Apricot.Assets.Models;

namespace Apricot.Assets;

/// <summary>
/// Abstraction over platform-specific artifacts. In apricot asset is a raw file without any preparation for engine.
/// <see cref="Artifact"/> is some result of import process produced by <see cref="IAssetsImporter"/>. It is platform
/// specific and stored in assets database.
///
/// The idea is that all required assets are pre-imported and converted to artifacts. In future database may bake
/// everything in advance at compile time and then import won't be required, but as of the right now import should be
/// called before actually using artifacts.
///
/// Asset database should not consider path as actual filesystem path. All file-specific work should be done by
/// <see cref="IAssetsImporter"/> and assets database is basically a dispatcher above importers. It may cache results,
/// put them in local database, but not too much of other logic. 
/// </summary>
/// <seealso cref="AssetsExtensions"/>
public interface IAssetsDatabase
{
    // /// <summary>
    // /// Imports asset at specified path with provided <see cref="ImportSettings"/>. Will delete all previously imported
    // /// assets that match settings and replace them with newly imported.
    // /// </summary>
    // /// <param name="path">Path or codename of asset.</param>
    // /// <param name="settings">Settings of operation.</param>
    // /// <returns>Guid constructed for specified asset path. It is guaranteed to be persistent between calls.</returns>
    // Guid Import(string path, ImportSettings settings);

    void BuildDatabase();
    
    /// <summary>
    /// Gets asset from provided path.
    /// </summary>
    /// <param name="assetUri">URI to asset.</param>
    /// <returns></returns>
    Asset GetAsset(Uri assetUri);
    
    /// <summary>
    /// Gets asset id for specified path. If no asset is present would return null.
    /// </summary>
    /// <param name="assetUri">URI to asset.</param>
    Guid? GetAssetId(Uri assetUri);
}
