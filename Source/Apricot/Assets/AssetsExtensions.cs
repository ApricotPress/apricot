using Apricot.Assets.Models;

namespace Apricot.Assets;

/// <summary>
/// Simple extensions for assets database to simplify work with it without introducing abstract classes.
/// </summary>
public static class AssetsExtensions
{
    /// <summary>
    /// Automatically imports assets (for all platforms), gets its id and returns artifact that corresponds to artifact
    /// target.
    /// </summary>
    /// <param name="assets">Assets database to use.</param>
    /// <param name="path">Path of the asset.</param>
    /// <param name="target">Target of artifact to use when filtering.</param>
    /// <returns>Corresponding artifact data.</returns>
    public static byte[] GetArtifact(this IAssetsDatabase assets, string path, ArtifactTarget target)
    {
        // todo: do not reimport
        var id = assets.Import(path, new ImportSettings(new ArtifactTarget()));
        
        return assets.GetArtifact(id, target);
    }
}
