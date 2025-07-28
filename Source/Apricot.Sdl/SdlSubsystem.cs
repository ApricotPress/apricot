using Apricot.Sdl.Windows;
using Apricot.Windows;
using Microsoft.Extensions.Logging;
using static SDL3.SDL;

namespace Apricot.Sdl;

// todo: check whether listeners are updated on hot reload
public class SdlSubsystem(
    ILogger<SdlSubsystem> logger,
    IWindowsManager windowsManager,
    IEnumerable<ISdlEventListener> sdlEventListeners
) : ISubsystem
{
    private readonly ISdlEventListener[] _listeners = sdlEventListeners.ToArray();

    public void Initialize()
    {
        logger.LogInformation("Initializing SDL");

        if (!SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO))
        {
            SdlException.ThrowFromLatest(nameof(SDL_Init));
        }

        logger.LogInformation("SDL version is {Version}", SDL_GetVersion());
    }

    private IntPtr _windowRenderer;

    public void BeforeFrame()
    {
        ReadEvents();

        if (_windowRenderer == IntPtr.Zero)
        {
            _windowRenderer = SDL_GetRenderer(((SdlWindow)windowsManager.GetOrCreateDefaultWindow()).Handle);
            if (_windowRenderer == IntPtr.Zero)
            {
                SdlException.ThrowFromLatest(nameof(SDL_GetRenderer));
            }
        }

        var now = SDL_GetTicks() / 1000f;
        var red = 0.5f + 0.5f * MathF.Sin(now);
        var green = 0.5f + 0.5f * MathF.Sin(now + MathF.PI * 2 / 3);
        var blue = 0.5f + 0.5f * MathF.Sin(now + MathF.PI * 4 / 3);

        SDL_SetRenderDrawColorFloat(_windowRenderer, red, green, blue, 1f);
        SDL_RenderClear(_windowRenderer);
        SDL_RenderPresent(_windowRenderer);
    }

    private void ReadEvents()
    {
        // todo: check whether this can be de-facto while (true) loop        
        while (SDL_PollEvent(out var evt))
        {
            foreach (var listener in _listeners)
            {
                listener.OnSdlEvent(evt);
            }
        }
    }
}
