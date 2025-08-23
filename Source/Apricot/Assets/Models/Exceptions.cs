namespace Apricot.Assets.Models;

public class AssetNotFoundException : Exception
{
    public AssetNotFoundException(string message) : base(message) { }

    public AssetNotFoundException(Guid assetId) : base($"Asset with id {assetId} is not found") { }
}
