using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Apricot.Graphics;
using Apricot.Graphics.Buffers;
using Apricot.Graphics.Textures;
using Apricot.Graphics.Vertecies;
using Apricot.Windows;
using Silk.NET.OpenGLES;
using Texture = Apricot.Graphics.Textures.Texture;

namespace Apricot.OpenGl;

public sealed class OpenGlGraphics(IGlPlatform glPlatform) : IGraphics
{
    private readonly Dictionary<IWindow, GlWindowTarget> _windowTargets = new();

    private GlWindowTarget? _currentWindow;

    public void Initialize(GraphicDriver preferredDriver, bool enableDebug) { }

    public void SetVsync(IWindow window, bool vsync) => glPlatform.SwapInterval = vsync ? 1 : 0;

    public IRenderTarget GetWindowRenderTarget(IWindow window)
    {
        if (_windowTargets.TryGetValue(window, out var target))
        {
            return target;
        }

        return _windowTargets[window] = new GlWindowTarget(window, glPlatform);
    }

    public Texture CreateTexture(
        string? name,
        int width,
        int height,
        TextureFormat format = TextureFormat.R8G8B8A8,
        TextureUsage usage = TextureUsage.Sampling
    )
    {
        throw new NotImplementedException();
    }

    public void SetTextureData(Texture texture, in ReadOnlySpan<byte> data) => throw new NotImplementedException();

    public void Release(Texture texture) => throw new NotImplementedException();

    public IndexBuffer CreateIndexBuffer(string? name, IndexSize indexSize, int capacity) =>
        throw new NotImplementedException();

    public void Release(IndexBuffer buffer) => throw new NotImplementedException();

    public VertexBuffer<T> CreateVertexBuffer<T>(string? name, int capacity)
        where T : unmanaged, IVertex => throw new NotImplementedException();

    public void Release<T>(VertexBuffer<T> buffer) where T : unmanaged, IVertex => throw new NotImplementedException();

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
        if (_currentWindow is not null)
        {
            glPlatform.SwapBuffers(_currentWindow.Window);
        }

        _currentWindow = null;
    }

    public void Dispose()
    {
        foreach (var windowTarget in _windowTargets.Values)
        {
            windowTarget.Dispose();
        }

        _windowTargets.Clear();
        _currentWindow = null;
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
