using Apricot.Graphics.Shaders;

namespace Apricot.Graphics.Materials;

public class Material
{
    public Material(ShaderProgram vertex, ShaderProgram fragment)
    {
        if (vertex.Stage != ShaderStage.Vertex)
        {
            throw new ArgumentException("Got wrong shader for vertex stage", nameof(vertex));
        }
        
        if (fragment.Stage != ShaderStage.Fragment)
        {
            throw new ArgumentException("Got wrong shader for fragment stage", nameof(vertex));
        }
        
        VertexStage = new Stage(vertex);
        FragmentStage = new Stage(fragment);
    }
    public Stage VertexStage { get; }

    public Stage FragmentStage { get; }
}
