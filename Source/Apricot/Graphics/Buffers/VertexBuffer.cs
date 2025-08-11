using Apricot.Graphics.Vertices;

namespace Apricot.Graphics.Buffers;

public class VertexBuffer(IGraphics graphics, string name, VertexFormat format, int capacity, IntPtr nativePointer)
    : GraphicBuffer(name, format.Stride, capacity, nativePointer, BufferUsage.Vertex)
{
    protected IGraphics Graphics { get; } = graphics;
    
    public VertexFormat Format { get; } = format;

    public override void Dispose()
    {
        if (IsDisposed) return;

        Graphics.Release(this);
        IsDisposed = true;
    }
}

public class VertexBuffer<T>(IGraphics graphics, string name, int capacity, IntPtr nativePointer)
    : VertexBuffer(graphics, name, T.Format, capacity, nativePointer)
    where T : unmanaged, IVertex
{
    public void UploadData(in ReadOnlySpan<T> vertices) => Graphics.UploadBufferData(this, vertices);
}
