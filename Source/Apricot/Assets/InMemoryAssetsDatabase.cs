using Microsoft.Extensions.Logging;

namespace Apricot.Assets;

/// <summary>
/// Implementation of <see cref="IAssetsDatabase"/> that simply stores all artifacts in-memory after import as a very
/// basic asset database.
///
/// Normalizes all provided paths before generating id.
/// </summary>
/// <param name="importers">List of all importers present in container.</param>
/// <param name="logger">Logger.</param>
public class InMemoryAssetsDatabase(
    IEnumerable<IAssetsImporter> importers,
    ILogger<InMemoryAssetsDatabase> logger
) : IAssetsDatabase
{
    private readonly IAssetsImporter[] _importers = importers.ToArray();
    private readonly Dictionary<string, Guid> _guidsCache = new();
    private readonly Dictionary<Guid, List<Artifact>> _artifacts = new();

    /// <inheritdoc />
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

                artifacts.Add(importer.Import(path, target));
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

    /// <inheritdoc />
    public Guid GetAssetId(string path)
    {
        var normalizedPath = NormalizePath(path);

        if (_guidsCache.TryGetValue(normalizedPath, out var guid)) return guid;

        var id = Guid.NewGuid();
        logger.LogInformation("Assigning Asset id {id} to {path}", id, normalizedPath);
        return _guidsCache[normalizedPath] = id;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<Artifact> GetArtifacts(Guid assetId) =>
        _artifacts.TryGetValue(assetId, out var artifacts)
            ? artifacts
            : Array.Empty<Artifact>();

    /// <inheritdoc />
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
