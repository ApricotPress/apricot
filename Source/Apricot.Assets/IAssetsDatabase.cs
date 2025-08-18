namespace Apricot.Assets;

public interface IAssetsDatabase
{
    Guid Import(string path, ImportSettings settings);

    Guid GetAssetId(string path);

    IReadOnlyCollection<Artifact> GetArtifacts(Guid assetId);

    byte[] GetArtifact(Guid assetId, ArtifactTarget query);
}
