namespace Apricot.Windows;

public interface IWindow : IDisposable
{
    event Action? OnResize;
    
    string Title { get; set; }

    int Width { get; set; }

    int Height { get; set; }
    
    bool IsFullscreen { get; set; }
    
    bool IsResizable { get; set; }
    
    bool IsAlwaysOnTop { get; set; }
}
