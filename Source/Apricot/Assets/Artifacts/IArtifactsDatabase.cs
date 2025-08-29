namespace Apricot.Assets.Artifacts;

/// <summary>
/// Database that looks for artifacts associated with assets and matches query.
/// </summary>
public interface IArtifactsDatabase
{
    /// <summary>
    /// Adds artifact to database.
    /// </summary>
    void Add(Asset asset, Artifact artifact);
    
    /// <summary>
    /// Looks for artifact associated with <paramref name="asset"/> that matches provided <paramref name="query"/> or
    /// null of no such artifacts exist in database.
    /// </summary>
    Artifact? FindArtifact(Asset asset, ArtifactTarget query);
}
