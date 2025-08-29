namespace Apricot.Assets.Artifacts;

/// <summary>
/// Cache of artifacts used by <see cref="CachedArtifactsDatabase"/>.
/// </summary>
public interface IArtifactsCache
{
    /// <summary>
    /// Adds artifact to cache.
    /// </summary>
    void Add(Asset asset, Artifact artifact);

    /// <summary>
    /// Returns enumerable with matched artifacts for <paramref name="asset"/>.
    /// </summary>
    IEnumerable<Artifact> GetArtifacts(Asset asset);
}
