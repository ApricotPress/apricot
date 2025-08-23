namespace Apricot.Assets.Artifacts;

public class CachedArtifactsDatabase(IEnumerable<IArtifactsCache> caches) : IArtifactsDatabase
{
    private readonly IArtifactsCache[] _caches = caches.ToArray();

    private IArtifactsCache MainCache => _caches[0];

    public Artifact FindArtifact(Guid assetId, ArtifactTarget artifactTarget)
    {
        foreach (var cache in _caches)
        {
            var artifacts = cache.GetArtifacts(assetId);

            foreach (var artifact in artifacts)
            {
                if (artifact.Target.Matches(artifactTarget))
                {
                    return artifact;
                }
            }
        }

        throw new KeyNotFoundException($"No artifact found for {assetId} asset with matching target");
    }
}
