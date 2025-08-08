using System;
using System.Diagnostics.CodeAnalysis;
using Apricot.Graphics;
using Apricot.Windows;
using Silk.NET.OpenGLES;

namespace Apricot.OpenGl;

public class OpenGlGraphics(IGlPlatform glPlatform) : IGraphics
{
    private GlWindowTarget? _currentWindow;

    public void Initialize() { }

    public void SetVsync(IWindow window, bool vsync) => glPlatform.SwapInterval = vsync ? 1 : 0;

    public IRenderTarget GetWindowRenderTarget(IWindow window) => new GlWindowTarget(window, glPlatform);

    public void SetRenderTarget(IRenderTarget target, Color? clearColor)
    {
        if (target is GlWindowTarget window)
        {
            _currentWindow = window;
        }
        else
        {
            throw new NotSupportedException("Currently on GlWindowTarget is supported as a renderer");
        }

        if (clearColor.HasValue)
        {
            Clear(clearColor.Value);
        }
    }

    public void Clear(Color color)
    {
        CheckCurrentWindow();

        _currentWindow.Gl.ClearColor(
            color.R,
            color.G,
            color.B,
            color.A
        );
        _currentWindow.Gl.Clear(ClearBufferMask.ColorBufferBit);
    }

    public void Present()
    {
    }

    [MemberNotNull(nameof(_currentWindow))]
    private void CheckCurrentWindow()
    {
        if (_currentWindow is null)
        {
            throw new InvalidOperationException("First set render target");
        }
    }
}
