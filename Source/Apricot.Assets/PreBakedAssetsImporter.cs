namespace Apricot.Assets;

/// <summary>
/// Simple importer with main purpose of built-in assets that holds all artifacts that were put in it using <see cref="AddPreBaked"/>.
/// </summary>
public class PreBakedAssetsImporter : IAssetsImporter
{
    private readonly List<PreBakedAsset> _preBakedList = [];

    /// <summary>
    /// Adds artifacts associated with given path or codename.
    /// </summary>
    /// <param name="path">Path or codename of asset.</param>
    /// <param name="artifacts">Artifacts that are associated with given path.</param>
    public void AddPreBaked(string path, Artifact[] artifacts) =>
        _preBakedList.Add(new PreBakedAsset(path, artifacts));

    /// <inheritdoc />
    public bool SupportsAsset(string path) => _preBakedList.Any(b => b.Path == path);

    /// <inheritdoc />
    public IEnumerable<ArtifactTarget> GetSupportedTargets(string path)
    {
        var asset = _preBakedList.First(b => b.Path == path);

        return asset.Artifacts.Select(a => a.Target).Distinct();
    }

    /// <inheritdoc />
    public Artifact Import(string path, ArtifactTarget target)
    {
        var asset = _preBakedList.First(b => b.Path == path);
        
        return asset.Artifacts.First(a => a.Target.Matches(target));
    }

    private record PreBakedAsset(string Path, Artifact[] Artifacts);
}
