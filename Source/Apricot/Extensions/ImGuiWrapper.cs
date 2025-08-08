using Apricot.Graphics;
using Apricot.Timing;
using Apricot.Windows;

namespace Apricot.Extensions;

public class ImGuiWrapper(IGraphics graphics, IWindowsManager windows, ITime time) : IJarLifecycleListener
{
    public ImGuiWindowRenderer? MainWindowRenderer { get; private set; }

    public void OnAfterInitialization()
    {
        MainWindowRenderer = new ImGuiWindowRenderer(
            graphics,
            windows.GetOrCreateDefaultWindow(),
            time
        );
        MainWindowRenderer.RebuildFontAtlas();
    }
}
