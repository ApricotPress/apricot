namespace Apricot.Graphics.Buffers;

public abstract class GraphicBuffer(
    string name,
    int capacity,
    int elementSize,
    IntPtr nativePointer,
    BufferUsage usage
) : IGraphicsResource
{
    public string Name { get; } = $"{usage}Buffer <{name}>";

    public int ElementSize { get; } = elementSize;

    public int Capacity { get; } = capacity;

    public IntPtr NativePointer { get; } = nativePointer;

    public BufferUsage Usage { get; } = usage;

    public bool IsDisposed { get; protected set; }

    public abstract void Dispose();
}
