namespace Apricot.Assets;

/// <summary>
/// Representation of platform-specific imported asset that is stored in assets database.
/// </summary>
/// <param name="Name"></param>
/// <param name="Target"></param>
/// <param name="Data"></param>
public record Artifact(string Name, ArtifactTarget Target, byte[] Data);
