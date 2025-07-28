using System.Diagnostics;
using Apricot.Scheduling;
using Apricot.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SDL3;

namespace Apricot.Sdl.Windows;

// todo: most of that class could be moved to abstract base class
public sealed class SdlWindowsManager(
    IMainThreadScheduler scheduler,
    ILogger<SdlWindowsManager> logger,
    IOptionsMonitor<DefaultWindowOptions> defaultWindowOptionsMonitor,
    ILoggerFactory loggerFactory
) : IWindowsManager, ISdlEventListener, IDisposable
{
    private SdlWindow? _defaultWindow;
    private IDisposable? _defaultWindowOptionsChange;

    private readonly Dictionary<uint, SdlWindow> _windows = [];

    public IEnumerable<IWindow> Windows => _windows.Values;

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

    private void Dispose(bool disposing)
    {
        if (!disposing) return;

        _defaultWindowOptionsChange?.Dispose();
        foreach (var window in _windows.Values)
        {
            window.Dispose();
        }
    }

    private SdlWindow Create(string title, int width, int height, WindowCreationFlags flags)
    {
        var window = new SdlWindow(title, width, height, flags, loggerFactory.CreateLogger<SdlWindow>());
        _windows[window.Id] = window;

        window.OnClose += OnWindowClosed;

        return window;
    }

    private void OnWindowClosed(IWindow window)
    {
        var sdlWindow = ((SdlWindow)window).Id;
        var removed = _windows.Remove(sdlWindow);

        Debug.Assert(removed, "Window was removed from dictionary");

        window.OnClose -= OnWindowClosed;

        if (window == _defaultWindow)
        {
            _defaultWindow = null;
        }
    }

    private void OnDefaultWindowOptionsChanged(DefaultWindowOptions options) =>
        scheduler.Schedule(() => OnDefaultWindowOptionsChangedUnsafe(options));

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
