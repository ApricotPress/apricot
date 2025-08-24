namespace Apricot.Assets.Artifacts;

/// <summary>
/// Cache of artifacts used by <see cref="CachedArtifactsDatabase"/>.
/// </summary>
public interface IArtifactsCache
{
    /// <summary>
    /// Returns enumerable with matched artifacts for <paramref name="assetId"/>.
    /// </summary>
    IEnumerable<Artifact> GetArtifacts(Guid assetId);
}
