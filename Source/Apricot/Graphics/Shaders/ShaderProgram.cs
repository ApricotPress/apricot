using Apricot.Graphics.Resources;

namespace Apricot.Graphics.Shaders;

public sealed class ShaderProgram(
    IGraphics graphics,
    string name,
    IntPtr handle,
    ShaderStage stage
) : IGraphicsResource
{
    public string Name { get; } = $"Shader <{name}>";

    public IntPtr Handle { get; } = handle;

    public ShaderStage Stage { get; } = stage;

    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        if (IsDisposed) return;

        graphics.Release(this);
        IsDisposed = true;
    }

    public override string ToString() => Name;
}
