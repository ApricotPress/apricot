using System.Diagnostics;
using Apricot.Windows;
using Microsoft.Extensions.Logging;
using static SDL3.SDL;

namespace Apricot.Sdl.Windows;

public class SdlWindow(IntPtr handle, ILogger<SdlWindow> logger) : IWindow
{
    public uint Id { get; } = SDL_GetWindowID(handle);
    
    public string Title
    {
        get => SDL_GetWindowTitle(handle);
        set
        {
            logger.LogDebug("Setting window ({Handle}) title to {Title}", handle, value);
            
            if (!SDL_SetWindowTitle(handle, value))
            {
                SdlException.ThrowFromLatest(nameof(SDL_SetWindowTitle));
            }
        }
    }

    public int Width
    {
        get => SDL_GetWindowSize(handle, out var w, out _)
            ? w
            : throw SdlException.GetFromLatest(nameof(SDL_GetWindowSize));
        set
        {
            logger.LogDebug("Setting window ({Handle}) width to {W}", handle, value);
            
            if (!SDL_SetWindowSize(handle, value, Height))
            {
                SdlException.ThrowFromLatest(nameof(SDL_SetWindowSize));
            }
        }
    }

    public int Height
    {
        get => SDL_GetWindowSize(handle, out _, out var h)
            ? h
            : throw SdlException.GetFromLatest(nameof(SDL_GetWindowSize));
        set
        {
            logger.LogDebug("Setting window ({Handle}) height to {W}", handle, value);
            
            if (!SDL_SetWindowSize(handle, Width, value))
            {
                SdlException.ThrowFromLatest(nameof(SDL_SetWindowSize));
            }
        }
    }

    public bool IsFullscreen
    {
        get => SDL_GetWindowFlags(handle).HasFlag(SDL_WindowFlags.SDL_WINDOW_FULLSCREEN);
        set
        {
            logger.LogDebug("Setting window ({Handle}) fullscreen flag to {Value}", handle, value);
            
            if (!SDL_SetWindowFullscreen(handle, value))
            {
                SdlException.ThrowFromLatest(nameof(SDL_SetWindowFullscreen));
            }
        }
    }

    public bool IsResizable
    {
        get => HasWindowFlag(SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
        set
        {
            logger.LogDebug("Setting window ({Handle}) resize flag to {Value}", handle, value);
            
            if (!SDL_SetWindowResizable(handle, value))
            {
                SdlException.ThrowFromLatest(nameof(SDL_SetWindowResizable));
            }
        }
    }

    public bool IsAlwaysOnTop
    {
        get => HasWindowFlag(SDL_WindowFlags.SDL_WINDOW_ALWAYS_ON_TOP);
        set
        {
            logger.LogDebug("Setting window ({Handle}) always on top flag to {Value}", handle, value);
            
            if (!SDL_SetWindowAlwaysOnTop(handle, value))
            {
                SdlException.ThrowFromLatest(nameof(SDL_SetWindowAlwaysOnTop));
            }
        }
    }

    public event Action? OnResize;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        SDL_DestroyWindow(handle);
    }
    
    private bool HasWindowFlag(SDL_WindowFlags flag) => SDL_GetWindowFlags(handle).HasFlag(flag);

    internal void OnSdlEvent(SDL_WindowEvent windowEvent)
    {
        Debug.Assert(windowEvent.windowID == Id);

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (windowEvent.type)
        {
            case SDL_EventType.SDL_EVENT_WINDOW_RESIZED:
                logger.LogTrace("Window ({Handle}) is resized to {W}x{H}", handle, windowEvent.data1, windowEvent.data2);
                OnResize?.Invoke();
                break;
            
            default:
                logger.LogTrace("Unhandled window event of type {Type}", windowEvent.type);
                break;
        }
    }
}
