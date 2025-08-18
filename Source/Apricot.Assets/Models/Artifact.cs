namespace Apricot.Assets;

public record Artifact(Guid AssetId, string Name, ArtifactTarget Target, byte[] Data);
