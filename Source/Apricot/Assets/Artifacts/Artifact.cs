using MessagePack;

namespace Apricot.Assets.Artifacts;

/// <summary>
/// Representation of platform-specific baked asset that is stored in assets database.
/// </summary>
/// <seealso cref="IArtifactsDatabase"/>
/// <seealso cref="IAssetDatabase"/>
/// <param name="AssetId">ID of asset associated with artifact.</param>
/// <param name="Target">Target that artifact should be used for.</param>
/// <param name="Data">Generic data that was produced on asset import process.</param>
[MessagePackObject(true)]
public record Artifact(
    Guid AssetId,
    ArtifactTarget Target,
    object Data
);
