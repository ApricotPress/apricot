namespace Apricot.Graphics.Buffers;

public sealed class VertexBuffer(IGraphics graphics, string name, int capacity, VertexFormat format, IntPtr nativePointer)
    : GraphicBuffer(name, capacity, format.Stride, nativePointer, BufferUsage.Vertex)
{
    public VertexFormat Format { get; } = format;
    
    public override void Dispose()
    {
        if (IsDisposed) return;

        graphics.Release(this);
        IsDisposed = true;
    }
}
