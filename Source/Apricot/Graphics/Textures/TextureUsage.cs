namespace Apricot.Graphics.Textures;

[Flags]
public enum TextureUsage
{
    None = 0,
    Sampling = 1,
    ColorTarget = 2,
    DepthStencilTarget = 4
}
