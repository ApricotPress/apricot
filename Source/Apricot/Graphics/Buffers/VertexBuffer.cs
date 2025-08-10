using Apricot.Graphics.Vertecies;

namespace Apricot.Graphics.Buffers;

public sealed class VertexBuffer<T>(IGraphics graphics, string name, int capacity, IntPtr nativePointer)
    : GraphicBuffer(name, capacity, T.Format.Stride, nativePointer, BufferUsage.Vertex)
    where T : unmanaged, IVertex
{
    public VertexFormat Format => T.Format;

    public override void Dispose()
    {
        if (IsDisposed) return;

        graphics.Release(this);
        IsDisposed = true;
    }
}
