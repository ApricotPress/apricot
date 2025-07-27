namespace Apricot.Windows;

public interface IWindow : IDisposable
{
    public string Title { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }
    
    bool IsFullscreen { get; set; }
    
    bool IsResizable { get; set; }
    
    bool IsAlwaysOnTop { get; set; }
}
