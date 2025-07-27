namespace Apricot.Windows;

public interface IWindowsManager
{
    IWindow Create(string title, int width, int height, WindowCreationFlags flags);

    IWindow GetOrCreateDefaultWindow();
}
