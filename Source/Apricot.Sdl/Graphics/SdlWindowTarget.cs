using Apricot.Graphics;
using Apricot.Sdl.Windows;
using SDL3;

namespace Apricot.Sdl.Graphics;

/// <summary>
/// Holds handle to window that then can be used to acquire its swapchain.
/// </summary>
public sealed class SdlWindowTarget : IRenderTarget
{
    private readonly SdlGraphics _graphics;

    /// <summary>
    /// Reference to SDL window. 
    /// </summary>
    public SdlWindow Window { get; }

    public SdlWindowTarget(SdlGraphics graphics, SdlWindow window)
    {
        if (!SDL.SDL_ClaimWindowForGPUDevice(graphics.GpuDeviceHandle, window.Handle))
        {
            SdlException.ThrowFromLatest(nameof(SDL.SDL_ClaimWindowForGPUDevice));
        }
        
        _graphics = graphics;
        Window = window;
    }

    /// <inheritdoc />
    public void Dispose() => SDL.SDL_ReleaseWindowFromGPUDevice(_graphics.GpuDeviceHandle, Window.Handle);
}
