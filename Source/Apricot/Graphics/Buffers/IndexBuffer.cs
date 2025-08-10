namespace Apricot.Graphics.Buffers;

public sealed class IndexBuffer(IGraphics graphics, string name, int capacity, IndexSize size, IntPtr nativePointer)
    : GraphicBuffer(name, capacity, (int)size, nativePointer, BufferUsage.Index)
{
    public IndexSize IndexSize { get; } = size;
    
    public override void Dispose()
    {
        if (IsDisposed) return;

        graphics.Release(this);
        IsDisposed = true;
    }

    public override string ToString() => Name;
}
