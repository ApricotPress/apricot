using Apricot.Graphics.Structs;
using Apricot.Lifecycle;
using Apricot.Lifecycle.TickHandlers;
using Apricot.Windows;

namespace Apricot.Graphics.GameLoop;

/// <summary>
/// Default pre-render tick handler that sets render target to main window.
/// </summary>
/// <seealso cref="DefaultGameLoopProvider"/>
public class PrepareRenderTickHandle(IGraphics graphics, IWindowsManager windows) : ITickHandler
{
    private readonly IRenderTarget _renderTarget = graphics.GetWindowRenderTarget(windows.GetOrCreateDefaultWindow());

    public void Tick()
    {
        graphics.SetRenderTarget(_renderTarget, Color.White);
    }
}
