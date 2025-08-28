using System.Diagnostics;
using Apricot.Assets.Artifacts;
using Apricot.Assets.Sources;
using Apricot.Utils;
using Microsoft.Extensions.Logging;

namespace Apricot.Assets.InMemory;

public class InMemoryAssetDatabase(
    IEnumerable<IAssetsSource> sources,
    IArtifactsDatabase artifactsDatabase,
    IEnumerable<IAssetsImporter> importers,
    ILogger<InMemoryAssetDatabase> logger
) : IAssetDatabase
{
    private readonly IAssetsImporter[] _importers = importers.ToArray();
    private readonly IAssetsSource[] _sources = sources.ToArray();
    private readonly Dictionary<Uri, Guid> _guidsCache = new();
    private readonly Dictionary<Guid, Asset> _assets = new();

    /// <inheritdoc />
    public void BuildDatabase()
    {
        logger.LogInformation("Building assets database...");

        foreach (var source in _sources)
        {
            logger.LogInformation("Listing assets present under {scheme} scheme", source.Scheme);

            var assets = source.ListAssetsPaths("", ListAssetsType.Recursive);

            foreach (var assetLocalPath in assets)
            {
                logger.LogInformation("Registering {scheme}:{asset}...", source.Scheme, assetLocalPath);

                var assetUri = new UriBuilder(source.Scheme, null, 0, assetLocalPath).Uri;
                var assetId = GetOrCreateId(assetUri);

                var asset = new Asset(
                    Path.GetFileNameWithoutExtension(assetLocalPath),
                    assetId,
                    assetUri
                );

                _assets[assetId] = asset;
            }
        }

        logger.LogInformation("Loaded {count} assets", _assets.Count);
    }

    public void ImportAssets()
    {
        foreach (var asset in _assets.Values)
        {
            logger.LogInformation("Importing {assetUri}", asset.Uri);

            foreach (var importer in _importers)
            {
                if (!importer.SupportsAsset(asset)) continue;

                var targets = importer.GetSupportedTargets(asset);

                foreach (var artifactTarget in targets)
                {
                    // we already have matching artifact in asset database
                    if (artifactsDatabase.FindArtifact(asset, artifactTarget) is not null)
                    {
                        logger.LogInformation(
                            "Already have artifact for asset {assetUri} with matching target {target}",
                            asset.Uri,
                            artifactTarget
                        );
                        continue;
                    }
                    
                    var artifact = importer.Import(asset, OpenAsset(asset.Uri), artifactTarget);
                    
                    Debug.Assert(artifact.AssetId == asset.Id);
                    artifactsDatabase.Add(artifact);
                }
            }
        }
    }

    /// <inheritdoc />
    public Asset GetAsset(Uri assetPath)
    {
        var id = GetAssetId(assetPath);

        if (id is null) throw NotFoundException();

        return _assets.TryGetValue(id.Value, out var asset)
            ? asset
            : throw NotFoundException();

        AssetNotFoundException NotFoundException() => new($"Asset {assetPath} was not found.");
    }

    /// <inheritdoc />
    public Guid? GetAssetId(Uri path) =>
        _guidsCache.TryGetValue(UriUtils.NormalizeAssetsUri(path), out var guid)
            ? guid
            : null;

    /// <inheritdoc />
    public Stream OpenAsset(Uri assetUri)
    {
        var scheme = assetUri.Scheme;

        if (string.IsNullOrWhiteSpace(scheme))
        {
            foreach (var source in _sources)
            {
                if (source.Exists(assetUri.LocalPath))
                {
                    return source.Open(assetUri.LocalPath);
                }
            }

            var sourcesList = string.Join(", ", sources.Select(s => s.Scheme));
            throw new AssetNotFoundException($"No asset found at URI: {assetUri}. Checked sources: {sourcesList}");
        }
        else
        {
            var source =
                _sources.FirstOrDefault(s => s.Scheme.Equals(scheme, StringComparison.InvariantCultureIgnoreCase));

            if (source is null)
            {
                throw new NotSupportedException($"No sources suitable for scheme were found: {assetUri.Scheme}");
            }

            return source.Open(assetUri.LocalPath);
        }
    }

    private Guid GetOrCreateId(Uri path)
    {
        if (GetAssetId(path) is { } guid) return guid;

        var normalized = UriUtils.NormalizeAssetsUri(path);

        var id = Guid.NewGuid();
        logger.LogInformation("Assigning Asset id {id} to {path}", id, normalized);
        return _guidsCache[normalized] = id;
    }
}
