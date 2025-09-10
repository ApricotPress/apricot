namespace Apricot.Assets.Importing;

/// <summary>
/// Some meta used for importing and loading assets by assets database. 
/// </summary>
/// <param name="Id">Optional id of asset that should be used. If not provided, asset database should assign own.</param>
public record AssetTag(
    Guid? Id
);
