using Apricot.Assets.Artifacts;
using Microsoft.Extensions.Logging;

namespace Apricot.Assets.InMemory;

public class InMemoryArtifactsCache(ILogger<InMemoryArtifactsCache> logger) : IArtifactsCache
{
    private readonly Dictionary<Guid, List<Artifact>> _artifacts = new();

    public void Add(Artifact artifact)
    {
        logger.LogInformation(
            "Adding artifact for asset {asset} with target {target}",
            artifact.AssetId,
            artifact.Target
        );
        GetArtifactsList(artifact.AssetId).Add(artifact);
    }

    public IEnumerable<Artifact> GetArtifacts(Asset asset) => GetArtifactsList(asset.Id);

    private List<Artifact> GetArtifactsList(Guid assetId) => _artifacts.TryGetValue(assetId, out var l)
        ? l
        : _artifacts[assetId] = [];
}
