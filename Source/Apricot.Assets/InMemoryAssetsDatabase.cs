using Microsoft.Extensions.Logging;

namespace Apricot.Assets;

public class InMemoryAssetsDatabase(
    IEnumerable<IAssetImporter> importers,
    ILogger<InMemoryAssetsDatabase> logger
) : IAssetsDatabase
{
    private readonly IAssetImporter[] _importers = importers.ToArray();
    private readonly Dictionary<string, Guid> _guidsCache = new();
    private readonly Dictionary<Guid, List<Artifact>> _artifacts = new();

    public Guid Import(string path, ImportSettings settings)
    {
        logger.LogInformation("Importing {path}...", path);

        var id = GetAssetId(path);
        var artifacts = new List<Artifact>();

        foreach (var importer in _importers)
        {
            if (!importer.SupportsAsset(path)) continue;

            var targets = importer.GetSupportedTargets(path);

            foreach (var target in targets)
            {
                if (!target.Matches(settings.Query)) continue;

                artifacts.Add(importer.Import(id, path, target));
            }
        }

        if (artifacts.Count == 0)
        {
            logger.LogWarning("No importers were found for {path} with specified settings", path);
        }

        if (!_artifacts.ContainsKey(id)) _artifacts[id] = [];

        _artifacts[id].RemoveAll(a => a.Target.Matches(settings.Query));
        _artifacts[id].AddRange(artifacts);

        return id;
    }

    public Guid GetAssetId(string path)
    {
        var normalizedPath = NormalizePath(path);

        if (_guidsCache.TryGetValue(normalizedPath, out var guid)) return guid;

        var id = Guid.NewGuid();
        logger.LogInformation("Assigning Asset id {id} to {path}", id, normalizedPath);
        return _guidsCache[normalizedPath] = id;
    }

    public IReadOnlyCollection<Artifact> GetArtifacts(Guid assetId) =>
        _artifacts.TryGetValue(assetId, out var artifacts)
            ? artifacts
            : Array.Empty<Artifact>();

    public byte[] GetArtifact(Guid assetId, ArtifactTarget query)
    {
        var artifacts = GetArtifacts(assetId);
        var artifact = artifacts.FirstOrDefault(a => a.Target.Matches(query));

        if (artifact is null)
        {
            throw new Exception($"Artifact {assetId} with query {query} was not found");
        }

        return artifact.Data;
    }

    private static string NormalizePath(string path) => Path
        .GetFullPath(path)
        .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
}
