using Apricot.Lifecycle.TickHandlers;

namespace Apricot.Graphics;

public class PresentGraphicsTickHandle(IGraphics graphics) : ITickHandler
{
    public void Tick() => graphics.Present();
}
