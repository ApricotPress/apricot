namespace Apricot.Assets;

public interface IAssetImporter
{
    bool SupportsAsset(string path);

    IEnumerable<ArtifactTarget> GetSupportedTargets(string path);

    Artifact Import(Guid id, string path, ArtifactTarget target);
}
