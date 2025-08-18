namespace Apricot.Assets;

/// <summary>
/// Settings for <see cref="IAssetsDatabase.Import"/> to use when importing.
/// </summary>
/// <param name="Query">Artifact target query for which artifacts should be produced.</param>
public record ImportSettings(
    ArtifactTarget Query
);
