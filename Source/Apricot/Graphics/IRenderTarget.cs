namespace Apricot.Graphics;

/// <summary>
/// Opaque reference to render target that is acquired from <see cref="IGraphics"/> implementation.
/// </summary>
public interface IRenderTarget : IGraphicsResource
{
    int Width { get; }
    
    int Height { get; }
}
