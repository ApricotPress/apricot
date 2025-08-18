namespace Apricot.Assets;

/// <summary>
/// Importer of assets that converts raw asset data to engine ready and platform specific data called artifact.
/// </summary>
/// <remarks>
/// Path to asset that is passed to importer may be absolute, relative, or simply a codename. Importer can decide on its
/// own what to do in each case at the moment.
/// </remarks>
public interface IAssetsImporter
{
    /// <summary>
    /// Should return true if asset located at path is supported by importer. It is not recommended to call other
    /// methods of importer without this check. 
    /// </summary>
    /// <param name="path">Path to asset. See class remarks.</param>
    /// <returns>True if asset located at path is supported.</returns>
    bool SupportsAsset(string path);

    /// <summary>
    /// Returns enumerable with collection of artifact targets that importer can produce for specific asset.  
    /// </summary>
    /// <param name="path">Path to asset. See class remarks.</param>
    /// <returns>Enumerable of artifact targets.</returns>
    IEnumerable<ArtifactTarget> GetSupportedTargets(string path);

    /// <summary>
    /// Imports asset at path and produces artifact of specific target that is preferably should be an element of
    /// <see cref="GetSupportedTargets"/>.
    /// </summary>
    /// <param name="path">Path to assets. See class remarks.</param>
    /// <param name="target">Target of artifact that is expected to be supported after import.</param>
    /// <returns>Produced artifact. If no artifact were produced it should throw.</returns>
    Artifact Import(string path, ArtifactTarget target);
}
