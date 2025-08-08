using Apricot.Graphics;
using Apricot.Windows;
using Silk.NET.OpenGLES;

namespace Apricot.OpenGl;

public class GlWindowTarget(IWindow window, IGlPlatform glPlatform) : IRenderTarget
{
    public IWindow Window { get; } = window;
    public GL Gl { get; } = GL.GetApi(new SilkApricotGlContext(window, glPlatform));
    
    public void Dispose() => Gl.Dispose();
}
