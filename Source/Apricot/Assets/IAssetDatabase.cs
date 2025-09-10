using Apricot.Assets.Artifacts;
using Apricot.Assets.Importing;
using Apricot.Assets.Sources;

namespace Apricot.Assets;

/// <summary>
/// Holds information about assets, their associated ids and manages import process if assets are updated. <br/>
/// <br/>
/// In apricot asset is a raw file without any preparation for engine. <see cref="Artifact"/> is some result of import
/// process produced by <see cref="IAssetsImporter"/>. It is platform specific and stored in assets database.<br/>
/// <br/>
/// The idea is that all required assets are pre-imported and converted to artifacts. In future database may bake
/// everything in advance at compile time and then import won't be required, but as of the right now import should be
/// called before actually using artifacts.
/// </summary>
/// <seealso cref="IAssetsSource"/>
public interface IAssetDatabase
{
    /// <summary>
    /// Extension of filename containing meta information. Asset database should expect those files accessible by the
    /// same local path in assets source with <i>.tag</i> appended. 
    /// </summary>
    public const string TagFileExtension = "tag";
    
    /// <summary>
    /// Loads list of all assets, imports them, and assigns ids to them for later use.
    /// </summary>
    void BuildDatabase();

    /// <summary>
    /// Imports all assets and produces artifacts.
    /// </summary>
    void ImportAssets();

    /// <summary>
    /// Gets asset from provided URI.
    /// </summary>
    Asset GetAsset(Uri assetUri);

    /// <summary>
    /// Gets asset id for specified path. If no asset is present would return null.
    /// </summary>
    Guid? GetAssetId(Uri assetUri);

    /// <summary>
    /// Opens stream to read asset located at <paramref name="assetUri"/>.
    /// </summary>
    Stream OpenAsset(Uri assetUri);
}
