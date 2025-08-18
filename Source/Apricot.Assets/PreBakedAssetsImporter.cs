namespace Apricot.Assets;

public class PreBakedAssetsImporter : IAssetImporter
{
    private readonly List<PreBakedAsset> _preBakedList = [];

    public void AddPreBaked(string path, Artifact[] artifacts) =>
        _preBakedList.Add(new PreBakedAsset(path, artifacts));

    public bool SupportsAsset(string path) => _preBakedList.Any(b => b.Path == path);

    public IEnumerable<ArtifactTarget> GetSupportedTargets(string path)
    {
        var asset = _preBakedList.First(b => b.Path == path);

        return asset.Artifacts.Select(a => a.Target).Distinct();
    }

    public Artifact Import(Guid id, string path, ArtifactTarget target)
    {
        var asset = _preBakedList.First(b => b.Path == path);
        var artifact = asset.Artifacts.First(a => a.Target.Matches(target)) with
        {
            AssetId = id
        };

        return artifact;
    }

    public record PreBakedAsset(string Path, Artifact[] Artifacts);
}
