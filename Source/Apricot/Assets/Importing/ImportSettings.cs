using Apricot.Assets.Artifacts;

namespace Apricot.Assets.Importing;

/// <summary>
/// Settings for <see cref="IAssetDatabase.Import"/> to use when importing.
/// </summary>
/// <param name="Query">Artifact target query for which artifacts should be produced.</param>
public record ImportSettings(
    ArtifactTarget Query
);
