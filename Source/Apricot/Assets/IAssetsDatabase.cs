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
    /// <summary>
    /// Imports asset at specified path with provided <see cref="ImportSettings"/>. Will delete all previously imported
    /// assets that match settings and replace them with newly imported.
    /// </summary>
    /// <param name="path">Path or codename of asset.</param>
    /// <param name="settings">Settings of operation.</param>
    /// <returns>Guid constructed for specified asset path. It is guaranteed to be persistent between calls.</returns>
    Guid Import(string path, ImportSettings settings);

    /// <summary>
    /// Gets asset id for specified path. If no id is assigned, yet, should generate it. Id should be persistent between
    /// calls.
    /// </summary>
    /// <param name="path">Path to asset.</param>
    /// <returns>Unique id of asset located at path.</returns>
    Guid GetAssetId(string path);

    /// <summary>
    /// Should return list of all artifacts that were imported for specified asset id.
    /// </summary>
    /// <param name="assetId">Unique asset id acquired after import of from <see cref="GetAssetId"/>.</param>
    /// <returns>List of all associated artifacts.</returns>
    IReadOnlyCollection<Artifact> GetArtifacts(Guid assetId);

    /// <summary>
    /// Gets artifact that matches artifact target query for specified asset id. Should be called after
    /// <see cref="Import"/>.
    /// </summary>
    /// <param name="assetId">Unique asset id acquired after <see cref="Import"/> of from call to <see cref="GetAssetId"/>.</param>
    /// <param name="query">Target for which we are targeting artifact.</param>
    /// <returns>Data of first found artifact that matches query. Should throw an exception if no artifacts matched.</returns>
    /// <seealso cref="ArtifactTarget.Matches"/>
    byte[] GetArtifact(Guid assetId, ArtifactTarget query);
}
