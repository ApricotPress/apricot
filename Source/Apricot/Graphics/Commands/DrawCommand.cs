using Apricot.Graphics.Buffers;
using Apricot.Graphics.Materials;
using Apricot.Graphics.Structs;

namespace Apricot.Graphics.Commands;

public struct DrawCommand(IRenderTarget target, Material material, VertexBuffer vertexBuffer)
{
    // todo: remove SetRenderTarget as we have draw command render target?
    public IRenderTarget Target { get; set; } = target;

    public Material Material { get; set; } = material;

    public VertexBuffer VertexBuffer { get; set; } = vertexBuffer;

    public int VerticesCount { get; set; }

    public int VerticesOffset { get; set; }

    public IndexBuffer? IndexBuffer { get; set; } = null;

    public int IndicesCount { get; set; }

    public int IndicesOffset { get; set; }

    public Rect? Viewport { get; set; } = null;

    public Rect? Scissors { get; set; } = null;

    public BlendMode BlendMode { get; set; } = BlendMode.Premultiply;

    public CullMode CullMode { get; set; } = CullMode.None;

    public DepthCompare DepthCompare { get; set; } = DepthCompare.Less;

    public bool DepthTestEnabled { get; set; } = false;

    public bool DepthWriteEnabled { get; set; } = false;
}
