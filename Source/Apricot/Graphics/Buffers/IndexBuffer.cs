namespace Apricot.Graphics.Buffers;

public sealed class IndexBuffer(IGraphics graphics, string name, int capacity, IndexSize size, IntPtr nativePointer)
    : GraphicBuffer(name, capacity, (int)size, nativePointer, BufferUsage.Index)
{
    public IndexSize IndexSize { get; } = size;

    public void UploadData(in ReadOnlySpan<int> indices)
    {
        if (IndexSize != IndexSize._4)
        {
            throw new NotSupportedException("You are trying to upload 4-byte indices to 2-bytes buffer.");
        }

        graphics.UploadBufferData(this, indices);
    }

    public void UploadData(in ReadOnlySpan<short> indices)
    {
        if (IndexSize != IndexSize._2)
        {
            throw new NotSupportedException("You are trying to upload 2-byte indices to 4-bytes buffer.");
        }

        graphics.UploadBufferData(this, indices);
    }
    
    public override void Dispose()
    {
        if (IsDisposed) return;

        graphics.Release(this);
        IsDisposed = true;
    }

    public override string ToString() => Name;
}
