using Apricot.Assets.Artifacts;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace Apricot.Assets;

/// <summary>
/// Cache of artifacts that uses LiteDb as a cache.
/// </summary>
public class LiteDbArtifactsCache : IArtifactsCache, IDisposable
{
    private readonly ILogger<LiteDbArtifactsCache> _logger;
    private readonly LiteDatabase _database;
    private readonly ILiteCollection<Artifact> _artifactsCollection;


    public LiteDbArtifactsCache(ILogger<LiteDbArtifactsCache> logger, ConnectionString connection)
    {
        _logger = logger;
        _database = new LiteDatabase(connection);
        _artifactsCollection = _database.GetCollection<Artifact>("artifacts");

        _artifactsCollection.EnsureIndex(x => x.AssetId);

        _logger.LogInformation(
            "Loaded artifacts database. Found {artifactsCount} artifacts",
            _artifactsCollection.LongCount()
        );
    }

    public IEnumerable<Artifact> GetArtifacts(Asset asset) => _artifactsCollection.Find(a => a.AssetId == asset.Id);

    public void Add(Artifact artifact)
    {
        _logger.LogInformation(
            "Adding artifact for asset {asset} with target {target}",
            artifact.AssetId,
            artifact.Target
        );
        _artifactsCollection.Insert(artifact);
    }

    public void Dispose() => _database.Dispose();
}
