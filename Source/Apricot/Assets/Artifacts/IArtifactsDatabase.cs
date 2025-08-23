namespace Apricot.Assets.Artifacts;

public interface IArtifactsDatabase
{
    Artifact FindArtifact(Guid assetId, ArtifactTarget artifactTarget);
}
