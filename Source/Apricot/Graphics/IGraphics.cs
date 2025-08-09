using Apricot.Graphics.Buffers;
using Apricot.Graphics.Textures;
using Apricot.Lifecycle;
using Apricot.Windows;

namespace Apricot.Graphics;

/// <summary>
/// This interface represents unified low-level graphics API.<br/>
/// <br/>
/// <see cref="Initialize"/> expected to be called from <see cref="Jar.DoInitialization"/>. Then in your
/// <see cref="GameLoop"/> should go render section where you first set render target which could be window target
/// you acquired previously from <see cref="GetWindowRenderTarget"/> or off-screen buffer (NB: not yet implemented)
/// and afterwards you can proceed to draw commands.
/// </summary>
public interface IGraphics : IDisposable
{
    /// <summary>
    /// Initialize graphic system of the engine.
    /// </summary>
    /// <param name="preferredDriver">Preferred driver to use. May be ignored.</param>
    /// <param name="enableDebug">Should be extra debugging enabled, if supported.</param>
    void Initialize(GraphicDriver preferredDriver, bool enableDebug);

    /// <summary>
    /// Sets or disabled VSync. If supported, it would set VSync property only for specific window, although it's not
    /// guaranteed.
    /// </summary>
    /// <param name="window">Window which should change vertical synchronization setting.</param>
    /// <param name="vsync">New value.</param>
    void SetVsync(IWindow window, bool vsync);

    /// <summary>
    /// Gets semi-opaque objects representing target render texture which then can be used in <see cref="SetRenderTarget"/>.
    /// </summary>
    /// <param name="window">Windows for which surface we create render target.</param>
    /// <returns>Render target of specified window.</returns>
    IRenderTarget GetWindowRenderTarget(IWindow window);

    /// <summary>
    /// Creates texture bound to this graphics controller.
    /// </summary>
    /// <param name="name">Optional name for texture.</param>
    /// <param name="width">Width of requested texture.</param>
    /// <param name="height">Height of requested texture.</param>
    /// <param name="format">Format of texture.</param>
    /// <param name="usage">Usages of the texture.</param>
    /// <returns>Created and bound texture.</returns>
    Texture CreateTexture(string? name, int width, int height, TextureFormat format = TextureFormat.R8G8B8A8, TextureUsage usage = TextureUsage.Sampling);

    void SetTextureData(Texture texture, in ReadOnlySpan<byte> data);
    
    /// <summary>
    /// Releases texture. Should be called only from <see cref="Texture"/> by its Dispose method.
    /// </summary>
    /// <param name="texture">Texture to dispose.</param>
    void Release(Texture texture);

    IndexBuffer CreateIndexBuffer(string? name, IndexSize indexSize);

    void Release(IndexBuffer buffer);

    IndexBuffer CreateVertexBuffer(string? name);
    
    void Release(VertexBuffer buffer);

    /// <summary>
    /// Sets current target for next draw commands. Render target should be reset in <see cref="Present"/> methof which
    /// is usually called by game loop post-render routine. 
    /// </summary>
    /// <seealso cref="Ids.PresentGraphics"/>
    /// <param name="target">Render target that should be used for draw commands.</param>
    /// <param name="clearColor">Optional clear color of target.</param>
    void SetRenderTarget(IRenderTarget target, Color? clearColor);

    /// <summary>
    /// Clears current render target ro specified color. If current target is not specified should throw an exception.
    /// </summary>
    /// <param name="color">Color to clear color buffer of render target</param>
    void Clear(Color color);

    /// <summary>
    /// Waits for all ongoing render routines, fences, command buffers, etc. and flushes them to corresponding render
    /// targets if needed. Also prepares state of graphic API for next frame.  
    /// </summary>
    void Present();
}
