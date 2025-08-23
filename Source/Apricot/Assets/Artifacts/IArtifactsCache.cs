namespace Apricot.Assets.Artifacts;

public interface IArtifactsCache
{
    IEnumerable<Artifact> GetArtifacts(Guid assetId);
}
