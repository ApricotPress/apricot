using Apricot.Assets;
using Apricot.Assets.Artifacts;
using Apricot.Graphics;
using Apricot.Graphics.Structs;
using StbImageSharp;

namespace Apricot.Essentials.Importers;

public class ImageImporter : IAssetsImporter
{
    private static readonly string[] SupportedFormats =
    {
        "png",
        "jpg",
        "jpeg",
        "tga",
        "bmp"
    };

    public bool SupportsAsset(Asset asset) =>
        SupportedFormats.Any(f => Path.GetExtension(asset.Uri.LocalPath).EndsWith(f));

    public IEnumerable<ArtifactTarget> GetSupportedTargets(Asset asset) => [ArtifactTarget.Any];

    public Artifact Import(Asset asset, Stream stream, ArtifactTarget target)
    {
        var stbImage = ImageResult.FromStream(stream);
        var image = new Image(stbImage.Width, stbImage.Height);

        for (var y = 0; y < image.Height; y++)
        for (var x = 0; x < image.Width; x++)
            image[x, y] = GetColorAt(x, y, stbImage);

        return new Artifact(asset.Id, ArtifactTarget.Any, image);
    }

    private static Color GetColorAt(int x, int y, ImageResult stbImage)
    {
        var bytesPerPixel = (int)stbImage.Comp;
        var start = (x + stbImage.Width * y) * bytesPerPixel;

        return new PackedColor(
            GetChannel(0),
            GetChannel(1),
            GetChannel(2),
            GetChannel(3)
        );

        byte GetChannel(int i) => i > bytesPerPixel
            ? (byte)0
            : stbImage.Data[start + i];
    }
}
