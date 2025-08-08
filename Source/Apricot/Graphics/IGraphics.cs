using Apricot.Windows;

namespace Apricot.Graphics;

public interface IGraphics
{
    void Initialize();

    void SetVsync(IWindow window, bool vsync);

    IRenderTarget GetWindowRenderTarget(IWindow window);

    void Clear(Color red);

    void SetRenderTarget(IRenderTarget target, Color? clearColor);

    void Present();
}
