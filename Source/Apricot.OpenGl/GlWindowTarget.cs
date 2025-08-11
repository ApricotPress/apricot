using System;
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

    public string Name => $"OpenGL Window Target <{Window}>";

    public int Width => Window.Width;
    
    public int Height => Window.Height;

    public bool IsDisposed { get; private set; }

    public GL Gl { get; } = GL.GetApi(new SilkApricotGlContext(window, glPlatform));

    public void Dispose()
    {
        if (IsDisposed) return;

        IsDisposed = true;
        Gl.Dispose();
    }

    public override string ToString() => Name;
}
