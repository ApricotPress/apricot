using Apricot.Assets;

namespace Apricot.Essentials.Assets;

/// <summary>
/// Ids of built-in assets that are shipped with essentials.
/// </summary>
public static class EssentialsIds
{
    public static class Shaders
    {
        public static readonly Uri StandardVertex =
            new("embedded:Apricot.Essentials/Shaders/Standard.hlsl#" + AssetUtils.VertexTag);

        public static readonly Uri StandardFragment =
            new("embedded:Apricot.Essentials/Shaders/Standard.hlsl#" + AssetUtils.FragmentTag);
    }

    public static class Textures
    {
        public static readonly Uri ApricotLogo = new("embedded:Apricot.Essentials/Textures/ApricotLogo.png");
    }
}
