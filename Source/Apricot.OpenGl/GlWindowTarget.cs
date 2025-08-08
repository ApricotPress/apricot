using Apricot.Graphics;
using Apricot.Windows;
using Silk.NET.OpenGLES;

namespace Apricot.OpenGl;

/// <summary>
/// Creates and hols context referencing one specific window.
/// </summary>
public sealed class GlWindowTarget(IWindow window, IGlPlatform glPlatform) : IRenderTarget
{
    public IWindow Window { get; } = window;
    public GL Gl { get; } = GL.GetApi(new SilkApricotGlContext(window, glPlatform));
    
    public void Dispose() => Gl.Dispose();
}
