namespace Apricot.Sdl.Graphics;

public readonly struct GpuSwapchainTexture(IntPtr texture, uint width, uint height)
{
    public IntPtr TextureHandle { get; } = texture;

    public uint Width { get; } = width;

    public uint Height { get; } = height;
}
