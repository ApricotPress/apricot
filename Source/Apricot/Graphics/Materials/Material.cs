using Apricot.Graphics.Shaders;

namespace Apricot.Graphics.Materials;

public class Material(ShaderProgram vertex, ShaderProgram fragment)
{
    public Stage VertexStage { get; } = new(vertex);

    public Stage FragmentStage { get; } = new(fragment);
}
