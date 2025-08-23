using MessagePack;

namespace Apricot.Assets.Embedded;

[MessagePackObject(true)]
public record struct EmbeddedArtifactManifest(
    Uri? AssetUri,
    Guid? AssetId,
    string ArtifactLogicalName
);
