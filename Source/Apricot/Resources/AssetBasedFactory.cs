using Apricot.Assets;
using Apricot.Assets.Artifacts;
using Apricot.Assets.Models;
using Apricot.Platform;

namespace Apricot.Resources;

public abstract class AssetBasedFactory<T>(
    IAssetsDatabase assets,
    IArtifactsDatabase artifacts,
    IPlatformInfo platform
) : IResourceFactory<T, Uri>
{
    public T Load(Uri assetUri)
    {
        var asset = assets.GetAsset(assetUri);
        var tags = assetUri.Fragment.Length > 0
            ? assetUri.Fragment[1..].Split(",")
            : [];

        var artifact = artifacts.FindArtifact(
            asset.Id,
            new ArtifactTarget(platform.Platform, platform.GraphicDriver, tags)
        );

        return Construct(asset, artifact);
    }

    protected abstract T Construct(Asset asset, Artifact artifact);
}
