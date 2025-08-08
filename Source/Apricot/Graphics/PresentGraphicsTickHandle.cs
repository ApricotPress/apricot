using Apricot.Lifecycle;
using Apricot.Lifecycle.TickHandlers;

namespace Apricot.Graphics;

/// <summary>
/// Default post-render tick handler that calls <see cref="IGraphics.Present"/> after rendering routines have finished.
/// </summary>
/// <seealso cref="DefaultGameLoopProvider"/>
public class PresentGraphicsTickHandle(IGraphics graphics) : ITickHandler
{
    public void Tick() => graphics.Present();
}
