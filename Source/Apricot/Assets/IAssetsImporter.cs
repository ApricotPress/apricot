using Apricot.Assets.Artifacts;
using Apricot.Resources;

namespace Apricot.Assets;

/// <summary>
/// Importer of assets that converts raw asset data to engine ready and platform specific data called artifact. Which is
/// then used by <see cref="IResourceFactory{T,TArg}"/> to create resource or manually.
/// </summary>
public interface IAssetsImporter
{
    /// <summary>
    /// Should return true if it can load asset based on its info, and false otherwise.
    /// </summary>
    bool SupportsAsset(Asset asset);

    /// <summary>
    /// Returns enumerable with collection of artifact targets that importer can produce for specific asset.  
    /// </summary>
    /// <returns>Enumerable of artifact targets.</returns>
    IEnumerable<ArtifactTarget> GetSupportedTargets(Asset asset);

    /// <summary>
    /// Imports asset at path and produces artifact of specific target that is preferably should be an element of
    /// <see cref="GetSupportedTargets"/>.
    /// </summary>
    /// <param name="asset">Asset info.</param>
    /// <param name="stream">Stream of asset file.</param>
    /// <param name="target">Target that describes required artifact.</param>
    /// <returns>Produced artifact. If no artifact were produced it should throw.</returns>
    Artifact Import(Asset asset, Stream stream, ArtifactTarget target);
}
