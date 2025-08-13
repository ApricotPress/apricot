using System.Diagnostics;
using Apricot.Utils;
using Apricot.Windows;
using Microsoft.Extensions.Logging;
using static SDL3.SDL;

namespace Apricot.Sdl.Windows;

public class SdlWindow : IWindow
{
    public IntPtr Handle;

    protected ILogger<SdlWindow> Logger;

    public uint Id { get; }

    public string Title
    {
        get => SDL_GetWindowTitle(Handle);
        set
        {
            Logger.LogDebug("Setting window ({Handle}) title to {Title}", Handle, value);

            if (!SDL_SetWindowTitle(Handle, value))
            {
                SdlException.ThrowFromLatest(nameof(SDL_SetWindowTitle));
            }
        }
    }

    public int Width
    {
        get => SDL_GetWindowSize(Handle, out var w, out _)
            ? w
            : throw SdlException.GetFromLatest(nameof(SDL_GetWindowSize));
        set
        {
            Logger.LogDebug("Setting window ({Handle}) width to {W}", Handle, value);

            if (!SDL_SetWindowSize(Handle, value, Height))
            {
                SdlException.ThrowFromLatest(nameof(SDL_SetWindowSize));
            }
        }
    }

    public int Height
    {
        get => SDL_GetWindowSize(Handle, out _, out var h)
            ? h
            : throw SdlException.GetFromLatest(nameof(SDL_GetWindowSize));
        set
        {
            Logger.LogDebug("Setting window ({Handle}) height to {W}", Handle, value);

            if (!SDL_SetWindowSize(Handle, Width, value))
            {
                SdlException.ThrowFromLatest(nameof(SDL_SetWindowSize));
            }
        }
    }

    public bool IsFullscreen
    {
        get => SDL_GetWindowFlags(Handle).HasFlag(SDL_WindowFlags.SDL_WINDOW_FULLSCREEN);
        set
        {
            Logger.LogDebug("Setting window ({Handle}) fullscreen flag to {Value}", Handle, value);

            if (!SDL_SetWindowFullscreen(Handle, value))
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
            Logger.LogDebug("Setting window ({Handle}) resize flag to {Value}", Handle, value);

            if (!SDL_SetWindowResizable(Handle, value))
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
            Logger.LogDebug("Setting window ({Handle}) always on top flag to {Value}", Handle, value);

            if (!SDL_SetWindowAlwaysOnTop(Handle, value))
            {
                SdlException.ThrowFromLatest(nameof(SDL_SetWindowAlwaysOnTop));
            }
        }
    }

    public event Action<IWindow>? OnResize;

    public event Action<IWindow>? OnClose;

    public SdlWindow(string title, int width, int height, WindowCreationFlags flags, ILogger<SdlWindow> logger)
    {
        Logger = logger;

        using var _ = logger.BeginScope("SdlWindow.ctor");
        logger.LogInformation("Creating window with title {Title} ({W}x{H}:{Flags})", title, width, height, flags);

        var sdlFlags = SDL_WindowFlags.SDL_WINDOW_HIGH_PIXEL_DENSITY;

        if (flags.Has(WindowCreationFlags.Fullscreen))
        {
            sdlFlags |= SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;
        }

        if (flags.Has(WindowCreationFlags.Resizable))
        {
            // todo: it would not follow actual required size
            // see: https://wiki.libsdl.org/SDL3/SDL_WindowFlags#remarks
            sdlFlags |= SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
        }

        if (flags.HasFlag(WindowCreationFlags.AlwaysOnTop))
        {
            sdlFlags |= SDL_WindowFlags.SDL_WINDOW_ALWAYS_ON_TOP;
        }

        logger.LogDebug("Constructed SDL flags: {Flags}", sdlFlags);

        Handle = CreateWindow();

        if (Handle == 0)
        {
            SdlException.ThrowFromLatest(nameof(SDL_CreateWindow));
        }

        Id = SDL_GetWindowID(Handle);

        logger.LogInformation("Created window with handle {Handle} and {Id}", Handle, Id);

        // SDL_CreateWindow does not work with web assembly so we create renderer which later be acquired via 
        // SDL_GetRenderer. Why? I don't know. Maybe I've compiled SDL somehow faulty
        //
        // If I try - it gives Uncaught RuntimeError: null function or function signature mismatch
        IntPtr CreateWindow()
        {
            if (OperatingSystem.IsBrowser())
            {
                SDL_CreateWindowAndRenderer(title, width, height, sdlFlags, out var handle, out var _);

                return handle;
            }
            else
            {
                return SDL_CreateWindow(title, width, height, sdlFlags);
            }
        }
    }

    ~SdlWindow() => Dispose(false);

    public void Close()
    {
        Logger.LogInformation("Closing window {Handle}", Handle);

        SDL_DestroyWindow(Handle);
        Handle = IntPtr.Zero;
    }

    public override string ToString() => $"SdlWindow <{Handle}, {Id}>";

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (Handle != IntPtr.Zero)
        {
            SDL_DestroyWindow(Handle);
        }
    }

    private bool HasWindowFlag(SDL_WindowFlags flag) => SDL_GetWindowFlags(Handle).HasFlag(flag);

    internal void OnSdlEvent(SDL_WindowEvent windowEvent)
    {
        Debug.Assert(windowEvent.windowID == Id);

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (windowEvent.type)
        {
            case SDL_EventType.SDL_EVENT_WINDOW_RESIZED:
                Logger.LogTrace("Window ({Handle}) is resized to {W}x{H}", Handle, windowEvent.data1,
                    windowEvent.data2);
                OnResize?.Invoke(this);
                break;

            case SDL_EventType.SDL_EVENT_WINDOW_DESTROYED:
                Logger.LogTrace("Window ({Handle}) is destroyed", Handle);
                OnClose?.Invoke(this);
                break;

            case SDL_EventType.SDL_EVENT_WINDOW_CLOSE_REQUESTED:
                Logger.LogTrace("Window ({Handle}) was asked to be closed", Handle);
                Close();
                break;

            default:
                Logger.LogTrace("Unhandled window event of type {Type}", windowEvent.type);
                break;
        }
    }
}
