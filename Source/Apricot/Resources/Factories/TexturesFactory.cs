using Apricot.Assets;
using Apricot.Assets.Artifacts;
using Apricot.Graphics;
using Apricot.Graphics.Structs;
using Apricot.Graphics.Textures;
using Apricot.Platform;

namespace Apricot.Resources.Factories;

public class TexturesFactory(
    IAssetDatabase assets,
    IArtifactsDatabase artifacts,
    IPlatformInfo platform,
    IGraphics graphics
) : AssetBasedFactory<Texture>(assets, artifacts, platform)
{
    protected override Texture Construct(Asset asset, Artifact artifact)
    {
        if (artifact.Data is not Image image)
        {
            throw new InvalidOperationException(
                $"Wrong artifact underlying type. Expected image but got {artifact.Data.GetType()}"
            );
        }

        var texture = graphics.CreateTexture(asset.Name, image.Width, image.Height);

        var textureData = new byte[image.Data.Length * 4];
        for (var i = 0; i < image.Data.Length; i++)
        {
            var color = (PackedColor)image.Data[i];

            textureData[i * 4 + 0] = color.R;
            textureData[i * 4 + 1] = color.G;
            textureData[i * 4 + 2] = color.B;
            textureData[i * 4 + 3] = color.A;
        }

        texture.SetData(textureData);

        return texture;
    }
}
