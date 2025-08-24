namespace Apricot.Assets.Artifacts;

/// <summary>
/// Database that looks for artifacts associated with assets and matches query.
/// </summary>
public interface IArtifactsDatabase
{
    /// <summary>
    /// Looks for artifact associated with <paramref name="assetId"/> that matches provided <paramref name="query"/>.
    /// </summary>
    /// <exception cref="ArtifactNotFoundException">If no artifact were found</exception>
    Artifact FindArtifact(Guid assetId, ArtifactTarget query);
}
