using System.Numerics;
using System.Runtime.InteropServices;
using Apricot.Graphics.Structs;
using Apricot.Graphics.Vertices;

namespace Apricot.Essentials.DearImGui;

/// <summary>
/// Vertex definition used by ImGui.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ImGuiVertex(Vector2 pos, Vector2 uv, PackedColor color) : IVertex
{
    public static VertexFormat Format { get; } = new([
        new VertexFormat.Element(0, VertexElementFormat.Vector2, false),
        new VertexFormat.Element(1, VertexElementFormat.Vector2, false),
        new VertexFormat.Element(2, VertexElementFormat.UByte4, true),
    ]);


    public Vector2 Position = pos;
    public Vector2 UV = uv;
    public PackedColor Color = color;
}
