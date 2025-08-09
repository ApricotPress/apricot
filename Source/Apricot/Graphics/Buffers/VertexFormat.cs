using Apricot.Utils.Collections;

namespace Apricot.Graphics.Buffers;

public readonly struct VertexFormat : IEquatable<VertexFormat>
{
    public StackList32<Element> Elements { get; }
    public int Stride { get; }

    public VertexFormat(in ReadOnlySpan<Element> elements, int stride = 0)
    {
        Elements = new StackList32<Element>(elements);

        if (stride == 0)
        {
            foreach (var it in Elements)
            {
                Stride += it.Format.Size();
            }
        }
        else
        {
            Stride = stride;
        }
    }

    public static bool operator ==(VertexFormat a, VertexFormat b) => a.Equals(b);

    public static bool operator !=(VertexFormat a, VertexFormat b) => !a.Equals(b);

    public override bool Equals(object? obj) => obj is VertexFormat f && f.Equals(this);

    public bool Equals(VertexFormat other) =>
        Stride == other.Stride && Elements.Span.SequenceEqual(other.Elements.Span);

    public override int GetHashCode() => Elements.Aggregate(Stride.GetHashCode(), HashCode.Combine);

    public readonly record struct Element(
        int Index,
        VertexElementFormat Format,
        bool Normalized = true
    );
}
