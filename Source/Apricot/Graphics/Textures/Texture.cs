namespace Apricot.Graphics.Textures;

public sealed class Texture(
    IGraphics graphics,
    string name,
    int width,
    int height,
    IntPtr handle,
    TextureFormat format
) : IGraphicsResource
{
    public string Name { get; } = $"Texture <{name}>";

    public bool IsDisposed { get; private set; }

    public int Width { get; } = width;

    public int Height { get; } = height;

    public IntPtr Handle { get; } = handle;

    public TextureFormat Format { get; } = format;

    public void SetData(Span<byte> data)
    {
        if (IsDisposed) throw new InvalidOperationException("Texture was disposed and cannot have new data.");

        graphics.SetTextureData(this, data);
    }

    public void Dispose()
    {
        IsDisposed = true;
        graphics.Release(this);
    }

    public override string ToString() => Name;

    public override int GetHashCode() => Handle.GetHashCode();
}
