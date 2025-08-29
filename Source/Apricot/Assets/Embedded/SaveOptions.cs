namespace Apricot.Assets.Embedded;

/// <summary>
/// Describes settings of <see cref="EmbeddedArtifactsCache"/> to make it able to patch project file and embedded artifacts. 
/// </summary>
/// <param name="ProjectFilePath">Path to .csproj where to embed assets.</param>
/// <param name="BuiltArtifactsDirectory">Path where to save manifests and artifacts.</param>
public record SaveOptions(
    string ProjectFilePath,
    string BuiltArtifactsDirectory
);
