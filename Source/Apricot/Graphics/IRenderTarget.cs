using Apricot.Graphics.Structs;

namespace Apricot.Graphics;

/// <summary>
/// Opaque reference to render target that is acquired from <see cref="IGraphics"/> implementation.
/// </summary>
public interface IRenderTarget : IGraphicsResource
{
    int Width { get; }
    
    int Height { get; }
    
    Rect Viewport => new(0, 0, Width, Height);
}
