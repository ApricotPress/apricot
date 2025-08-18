namespace Apricot.Assets;

public static class AssetsExtensions
{
    public static byte[] GetArtifact(this IAssetsDatabase assets, string path, ArtifactTarget target)
    {
        // todo: do not reimport
        var id = assets.Import(path, new ImportSettings(new ArtifactTarget()));
        
        return assets.GetArtifact(id, target);
    }
}
