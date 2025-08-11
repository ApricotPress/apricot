using Apricot.Graphics.Shaders;

namespace Apricot.Graphics.Materials;

public class Material(ShaderProgram fragment, ShaderProgram vertex)
{
    public Stage FragmentStage { get; } = new(fragment);

    public Stage VertexStage { get; } = new(vertex);
}
