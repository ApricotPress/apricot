using System.Numerics;
using System.Runtime.InteropServices;

namespace Apricot.Graphics.Vertices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PositionTextureVertex(Vector3 position, Vector2 uv) : IVertex
{
    public static VertexFormat Format { get; } = new([
        new VertexFormat.Element(0, VertexElementFormat.Vector3, false),
        new VertexFormat.Element(1, VertexElementFormat.Vector2, false)
    ]);


    public Vector3 Position = position;
    public Vector2 UV = uv;
}
