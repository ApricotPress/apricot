namespace Apricot.Assets.Artifacts;

/// <summary>
/// Database that would use various <see cref="IArtifactsCache">caches</see> as artifacts sources. For
/// <see cref="IAssetDatabase">assets database</see> to add new artifact it would use first element of provided
/// <paramref name="caches"/> as a main cache and would ask it to save newly created artifacts there.
/// </summary>
public class CachedArtifactsDatabase(IEnumerable<IArtifactsCache> caches) : IArtifactsDatabase
{
    private readonly IArtifactsCache[] _caches = caches.ToArray();

    private IArtifactsCache MainCache => _caches[0];

    /// <inheritdoc />
    public void Add(Asset asset, Artifact artifact) => MainCache.Add(asset, artifact);

    /// <inheritdoc />
    public Artifact? FindArtifact(Asset asset, ArtifactTarget query)
    {
        foreach (var cache in _caches)
        {
            var artifacts = cache.GetArtifacts(asset);

            foreach (var artifact in artifacts)
            {
                if (artifact.Target.Matches(query))
                {
                    return artifact;
                }
            }
        }

        return null;
    }
}
