namespace Apricot.Graphics.Vertices;

public enum VertexElementFormat
{
    None,
    Single,
    Vector2,
    Vector3,
    Vector4,
    Byte4,
    UByte4,
    Short2,
    UShort2,
    Short4,
    UShort4
}

public static class VertexElementFormatExtensions
{
    public static int Size(this VertexElementFormat type) => type switch
    {
        VertexElementFormat.Single => 4,
        VertexElementFormat.Vector2 => 8,
        VertexElementFormat.Vector3 => 12,
        VertexElementFormat.Vector4 => 16,
        VertexElementFormat.Byte4 => 4,
        VertexElementFormat.UByte4 => 4,
        VertexElementFormat.Short2 => 4,
        VertexElementFormat.UShort2 => 4,
        VertexElementFormat.Short4 => 8,
        VertexElementFormat.UShort4 => 8,
        _ => throw new NotImplementedException(),
    };
    
    public static int ComponentsCount(this VertexElementFormat type) => type switch
    {
        VertexElementFormat.Single => 1,
        VertexElementFormat.Vector2 => 2,
        VertexElementFormat.Vector3 => 3,
        VertexElementFormat.Vector4 => 4,
        VertexElementFormat.Byte4 => 4,
        VertexElementFormat.UByte4 => 4,
        VertexElementFormat.Short2 => 2,
        VertexElementFormat.UShort2 => 2,
        VertexElementFormat.Short4 => 4,
        VertexElementFormat.UShort4 => 4,
        _ => throw new NotImplementedException(),
    };
}
