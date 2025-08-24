using MessagePack;

namespace Apricot.Assets.Embedded;

/// <summary>
/// Describes artifact that was embedded into DLL using EmbeddedResource MSBuild items. See Apricot.Essentials project
/// as example.
/// </summary>
/// <param name="AssetUri">Optional uri of associated asset.</param>
/// <param name="AssetId">Optional id of associated asset.</param>
/// <param name="ArtifactLogicalName">Logical name of asset that holds serialized artifact.</param>
/// <seealso cref="EmbeddedArtifactsCache"/>
[MessagePackObject(true)]
public record struct EmbeddedArtifactManifest(
    Uri? AssetUri,
    Guid? AssetId,
    string ArtifactLogicalName
);
