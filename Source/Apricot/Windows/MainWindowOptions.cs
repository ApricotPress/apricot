namespace Apricot.Windows;

/// <summary>
/// Specifies default options for main window. 
/// </summary>
/// <remarks>
/// <see cref="IWindowsManager"/> is expected to subscribe to configuration change and reconfigure window on option
/// change.
/// </remarks>
public class MainWindowOptions
{
    /// <summary>
    /// Window title.
    /// </summary>
    public string Title { get; set; } = "Game";

    /// <summary>
    /// Window starting width.
    /// </summary>
    public int Width { get; set; } = 800;

    /// <summary>
    /// Window starting height.
    /// </summary>
    public int Height { get; set; } = 600;

    /// <summary>
    /// Flags to use during creationg.
    /// </summary>
    public WindowCreationFlags Flags { get; set; } = WindowCreationFlags.None;
}
