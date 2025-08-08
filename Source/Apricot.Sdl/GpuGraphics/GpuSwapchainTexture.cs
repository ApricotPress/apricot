namespace Apricot.Sdl.Graphics;

/// <summary>
/// Holds data about swapchain texture acquired from SDL.
/// </summary>
/// <see cref="SDL3.SDL.SDL_AcquireGPUSwapchainTexture"/>
public readonly struct GpuSwapchainTexture(IntPtr texture, uint width, uint height)
{
    public IntPtr TextureHandle { get; } = texture;

    public uint Width { get; } = width;

    public uint Height { get; } = height;
}
