namespace Apricot.Windows;

/// <summary>
/// Represents game window or canvas if running in browser. 
/// </summary>
public interface IWindow : IDisposable
{
    event Action<IWindow>? OnResize;
    
    event Action<IWindow>? OnClose;
    
    /// <summary>
    /// Window current title. Should be only accessible from main thread.
    /// </summary>
    string Title { get; set; }

    /// <summary>
    /// Window current width. Should be only accessible from main thread.
    /// </summary>
    int Width { get; set; }

    /// <summary>
    /// Window current height. Should be only accessible from main thread.
    /// </summary>
    int Height { get; set; }
    
    /// <summary>
    /// Is window in fullscreen mode. Should be only accessible from main thread.
    /// </summary>
    bool IsFullscreen { get; set; }
    
    /// <summary>
    /// Is window resizable. Should be only accessible from main thread.
    /// </summary>
    bool IsResizable { get; set; }
    
    /// <summary>
    /// Is window always on top. Should be only accessible from main thread.
    /// </summary>
    bool IsAlwaysOnTop { get; set; }
    
    /// <summary>
    /// Closes window. Should trigger <see cref="OnClose"/> but not necessarily instantly. Should be called from main
    /// thread.
    /// </summary>
    void Close();
}
