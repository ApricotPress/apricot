using Apricot.Assets.Artifacts;

namespace Apricot.Assets;

public class AssetNotFoundException : Exception
{
    public AssetNotFoundException(string message) : base(message) { }

    public AssetNotFoundException(Guid assetId) : base($"Asset with id {assetId} is not found") { }
}

public class ArtifactNotFoundException(Guid assetId, ArtifactTarget query)
    : Exception($"No artifact for {assetId} asset found matching query\nQuery {query}");
