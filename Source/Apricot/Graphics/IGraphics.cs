using Apricot.Windows;

namespace Apricot.Graphics;

public interface IGraphics
{
    void Initialize();

    IRenderTarget GetWindowRenderTarget(IWindow window);

    void SetRenderTarget(IRenderTarget target, Color? clearColor);

    void Present();
}
