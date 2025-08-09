namespace Apricot.Graphics.Buffers;

public sealed class IndexBuffer(IGraphics graphics, string name, int capacity, IndexSize size, IntPtr nativePointer)
    : GraphicBuffer(name, capacity, (int)size, nativePointer, BufferUsage.Index)
{
    public override void Dispose()
    {
        if (IsDisposed) return;

        IsDisposed = true;
        graphics.Release(this);
    }
}
