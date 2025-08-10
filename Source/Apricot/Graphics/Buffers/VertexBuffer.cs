using Apricot.Graphics.Vertices;

namespace Apricot.Graphics.Buffers;

public sealed class VertexBuffer<T>(IGraphics graphics, string name, int capacity, IntPtr nativePointer)
    : GraphicBuffer(name, capacity, T.Format.Stride, nativePointer, BufferUsage.Vertex)
    where T : unmanaged, IVertex
{
    public VertexFormat Format => T.Format;

    public void UploadData(in ReadOnlySpan<T> vertices) => graphics.UploadBufferData(this, vertices);

    public override void Dispose()
    {
        if (IsDisposed) return;

        graphics.Release(this);
        IsDisposed = true;
    }
}
