namespace Apricot.Graphics.Textures;

public sealed class Texture(
    IGraphics graphics,
    string name,
    int width,
    int height,
    IntPtr handle,
    TextureFormat format,
    TextureUsage usage = TextureUsage.Sampling
) : IGraphicsResource
{
    public string Name { get; } = $"Texture <{name}>";

    public bool IsDisposed { get; private set; }

    public int Width { get; } = width;

    public int Height { get; } = height;

    public IntPtr Handle { get; } = handle;

    public TextureFormat Format { get; } = format;
    
    public void SetData(Span<byte> data) => graphics.SetTextureData(this, data);

    public void Dispose()
    {
        IsDisposed = true;
        graphics.ReleaseTexture(this);
    }

    public override string ToString() => Name;

    public override int GetHashCode() => Handle.GetHashCode();
}
