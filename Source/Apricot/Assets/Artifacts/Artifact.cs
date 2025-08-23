using MessagePack;

namespace Apricot.Assets.Artifacts;

/// <summary>
/// Representation of platform-specific imported asset that is stored in assets database.
/// </summary>
/// <param name="Target"></param>
/// <param name="Data"></param>
[MessagePackObject(true)]
public record Artifact(
    Guid AssetId,
    ArtifactTarget Target,
    object Data
);
