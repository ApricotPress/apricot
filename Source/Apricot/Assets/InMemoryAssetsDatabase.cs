using Apricot.Assets.Models;
using Apricot.Assets.Sources;
using Apricot.Utils;
using Microsoft.Extensions.Logging;

namespace Apricot.Assets;

public class InMemoryAssetsDatabase(
    IEnumerable<IAssetsSource> sources,
    ILogger<InMemoryAssetsDatabase> logger
) : IAssetsDatabase
{
    private readonly IAssetsSource[] _sources = sources.ToArray();
    private readonly Dictionary<Uri, Guid> _guidsCache = new();
    private readonly Dictionary<Guid, Asset> _assets = new();

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

                var fullPath = new Uri($"{source.Scheme}:{assetLocalPath}");
                var assetId = GetOrCreateId(fullPath);

                var asset = new Asset(
                    Path.GetFileNameWithoutExtension(assetLocalPath),
                    assetId
                );

                _assets[assetId] = asset;
            }
        }

        logger.LogInformation("Loaded {count} assets", _assets.Count);
    }

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
    public Guid? GetAssetId(Uri path)
    {
        var normalized = UriUtils.NormalizeAssetsUri(path);

        return _guidsCache.TryGetValue(normalized, out var guid)
            ? guid
            : null;
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
