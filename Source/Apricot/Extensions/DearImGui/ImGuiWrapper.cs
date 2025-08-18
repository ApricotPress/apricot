using Apricot.Assets;
using Apricot.Graphics;
using Apricot.Platform;
using Apricot.Timing;
using Apricot.Windows;

namespace Apricot.Extensions.DearImGui;

/// <summary>
/// Simple wrapper around ImGui that creates <see cref="ImGuiWindowRenderer"/> for main window which then can be used in
/// game loop.
/// </summary>
public class ImGuiWrapper(
    IGraphics graphics,
    IWindowsManager windows,
    ITime time,
    IAssetsDatabase assets,
    IPlatformInfo platform
) : IJarLifecycleListener
{
    public ImGuiWindowRenderer? MainWindowRenderer { get; private set; }

    public void OnAfterInitialization()
    {
        MainWindowRenderer = new ImGuiWindowRenderer(
            graphics,
            windows.GetOrCreateDefaultWindow(),
            time,
            assets,
            platform
        );
        MainWindowRenderer.RebuildFontAtlas();
    }
}
