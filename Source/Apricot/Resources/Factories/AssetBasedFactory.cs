using Apricot.Assets;
using Apricot.Assets.Artifacts;
using Apricot.Platform;

namespace Apricot.Resources.Factories;

/// <summary>
/// Generic factory that loads artifact associated with current running platform and graphic device. Uses assets URI
/// fragment as comma separated tags list for searching for artifact. 
/// </summary>
/// <typeparam name="T">Produced resource type</typeparam>
public abstract class AssetBasedFactory<T>(
    IAssetDatabase assets,
    IArtifactsDatabase artifacts,
    IPlatformInfo platform
) : IResourceFactory<T, Uri>
{
    /// <inheritdoc />
    public T Load(Uri assetUri)
    {
        var asset = assets.GetAsset(assetUri);
        var tags = assetUri.Fragment.Length > 0
            ? assetUri.Fragment[1..].Split(",")
            : [];

        // todo: should be auto-imported if not found and not stored in database all the time to avoid memory "leaks"
        var artifactTarget = new ArtifactTarget(platform.Platform, platform.GraphicDriver, tags);
        var artifact = artifacts.FindArtifact(asset, artifactTarget);

        if (artifact is null) throw new ArtifactNotFoundException(asset.Id, artifactTarget);

        return Construct(asset, artifact);
    }

    /// <summary>
    /// Should construct resource out of found asset and artifact.
    /// </summary>
    protected abstract T Construct(Asset asset, Artifact artifact);
}
