using Apricot.Scheduling;
using Apricot.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SDL3;

namespace Apricot.Sdl.Windows;

public class SdlWindowsManager(
    IScheduler scheduler,
    ILogger<SdlWindowsManager> logger,
    IOptionsMonitor<DefaultWindowOptions> defaultWindowOptionsMonitor,
    ILoggerFactory loggerFactory
) : IWindowsManager, ISdlEventListener, IDisposable
{
    private SdlWindow? _defaultWindow;
    private IDisposable? _defaultWindowOptionsChange;

    private readonly Dictionary<uint, SdlWindow> _windows = [];

    IWindow IWindowsManager.Create(string title, int width, int height, WindowCreationFlags flags) =>
        Create(title, width, height, flags);

    public IWindow GetOrCreateDefaultWindow()
    {
        if (_defaultWindow is not null) return _defaultWindow;

        logger.LogInformation("Default window was requested but not created. Creating it and subscribing for options");

        var currentOptions = defaultWindowOptionsMonitor.CurrentValue;

        _defaultWindow = Create(
            currentOptions.Title,
            currentOptions.Width,
            currentOptions.Height,
            currentOptions.Flags
        );

        // todo: cries from closures
        _defaultWindowOptionsChange = defaultWindowOptionsMonitor.OnChange(OnDefaultWindowOptionsChanged);

        return _defaultWindow;
    }

    public void OnSdlEvent(SDL.SDL_Event evt)
    {
        if ((evt.type & 0x200) != 0x200) return; // all window events are inside 0x200

        var windowEvent = evt.window;
        var window = _windows[evt.window.windowID];

        window.OnSdlEvent(windowEvent);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;

        _defaultWindowOptionsChange?.Dispose();
        _defaultWindow?.Dispose();
    }

    protected virtual SdlWindow Create(string title, int width, int height, WindowCreationFlags flags)
    {
        using var _ = logger.BeginScope(nameof(Create));
        logger.LogInformation("Creating window with title {Title} ({W}x{H}:{Flags})", title, width, height, flags);

        var sdlFlags = SDL.SDL_WindowFlags.SDL_WINDOW_HIGH_PIXEL_DENSITY;

        if (flags.HasFlag(WindowCreationFlags.Fullscreen))
        {
            sdlFlags |= SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;
        }

        if (flags.HasFlag(WindowCreationFlags.Resizable))
        {
            // todo: it would not follow actual required size
            // see: https://wiki.libsdl.org/SDL3/SDL_WindowFlags#remarks
            sdlFlags |= SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
        }

        if (flags.HasFlag(WindowCreationFlags.AlwaysOnTop))
        {
            sdlFlags |= SDL.SDL_WindowFlags.SDL_WINDOW_ALWAYS_ON_TOP;
        }

        if (!SDL.SDL_CreateWindowAndRenderer(title, width, height, sdlFlags, out IntPtr windowHandle, out var _))
        {
            SdlException.ThrowFromLatest(nameof(SDL.SDL_CreateWindow));
        }

        var window = new SdlWindow(windowHandle, loggerFactory.CreateLogger<SdlWindow>());
        _windows[window.Id] = window;

        return window;
    }

    private void OnDefaultWindowOptionsChanged(DefaultWindowOptions options)
    {
        scheduler.ScheduleOnMainThread(() => OnDefaultWindowOptionsChangedUnsafe(options));
    }

    private void OnDefaultWindowOptionsChangedUnsafe(DefaultWindowOptions options)
    {
        if (_defaultWindow is null) return;

        logger.LogInformation("Default window options changed. Updating window");

        if (_defaultWindow.Title != options.Title)
        {
            _defaultWindow.Title = options.Title;
        }

        if (_defaultWindow.Width != options.Width)
        {
            _defaultWindow.Width = options.Width;
        }

        if (_defaultWindow.Height != options.Height)
        {
            _defaultWindow.Height = options.Height;
        }

        // todo: refactor this circus somehow
        if (_defaultWindow.IsFullscreen != options.Flags.HasFlag(WindowCreationFlags.Fullscreen))
        {
            _defaultWindow.IsFullscreen = options.Flags.HasFlag(WindowCreationFlags.Fullscreen);
        }

        if (_defaultWindow.IsResizable != options.Flags.HasFlag(WindowCreationFlags.Resizable))
        {
            _defaultWindow.IsResizable = options.Flags.HasFlag(WindowCreationFlags.Resizable);
        }

        if (_defaultWindow.IsAlwaysOnTop != options.Flags.HasFlag(WindowCreationFlags.AlwaysOnTop))
        {
            _defaultWindow.IsAlwaysOnTop = options.Flags.HasFlag(WindowCreationFlags.AlwaysOnTop);
        }
    }
}
