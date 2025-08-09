using Apricot.Graphics;
using Apricot.Sdl.Windows;
using SDL3;

namespace Apricot.Sdl.Graphics;

/// <summary>
/// Holds handle to window that then can be used to acquire its swapchain.<br/>
/// <br/>
/// This class should not be constructed more than once for a window as it would try to claim window for GPU work twice.
/// </summary>
public sealed class SdlGpuWindowTarget : IRenderTarget
{
    private readonly SdlGpuGraphics _graphics;

    /// <summary>
    /// Reference to SDL window. 
    /// </summary>
    public SdlWindow Window { get; }

    public string Name => $"SDL Window Target <{Window}>";

    public bool IsDisposed { get; private set; }

    public SdlGpuWindowTarget(SdlGpuGraphics graphics, SdlWindow window)
    {
        if (!SDL.SDL_ClaimWindowForGPUDevice(graphics.GpuDeviceHandle, window.Handle))
        {
            SdlException.ThrowFromLatest(nameof(SDL.SDL_ClaimWindowForGPUDevice));
        }

        _graphics = graphics;
        Window = window;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (IsDisposed) return;

        IsDisposed = true;
        SDL.SDL_ReleaseWindowFromGPUDevice(_graphics.GpuDeviceHandle, Window.Handle);
    }

    public override string ToString() => Name;
}
