using Apricot.Lifecycle.TickHandlers;
using Apricot.Windows;

namespace Apricot.Graphics;

public class PrepareRenderTickHandle(IGraphics graphics, IWindowsManager windows) : ITickHandler
{
    private readonly IRenderTarget _renderTarget = graphics.GetWindowRenderTarget(windows.GetOrCreateDefaultWindow());

    public void Tick()
    {
        graphics.SetRenderTarget(_renderTarget, Color.White);
    }
}
