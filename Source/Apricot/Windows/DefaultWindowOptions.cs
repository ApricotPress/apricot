namespace Apricot.Windows;

public class DefaultWindowOptions
{
    public string Title { get; set; } = "Game";

    public int Width { get; set; } = 800;

    public int Height { get; set; } = 600;

    public WindowCreationFlags Flags { get; set; } = WindowCreationFlags.None;
}
