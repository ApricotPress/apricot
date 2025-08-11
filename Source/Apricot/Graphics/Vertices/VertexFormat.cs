using Apricot.Utils.Collections;

namespace Apricot.Graphics.Vertices;

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

    public bool Equals(VertexFormat other)
    {
        if (Stride != other.Stride) return false;
        if (other.Elements.Count != Elements.Count) return false;

        // dont use LINQ - it would generate garbage
        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var i = 0; i < other.Elements.Count; i++)
        {
            if (other.Elements[i] != Elements[i]) return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        var hash = Stride.GetHashCode();

        // dont use LINQ/foreach - it would generate garbage
        // ReSharper disable once ForCanBeConvertedToForeach
        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var i = 0; i < Elements.Count; i++)
        {
            hash = HashCode.Combine(hash, Elements[i].GetHashCode());
        }

        return hash;
    }

    public readonly record struct Element(
        int Location,
        VertexElementFormat Format,
        bool Normalized
    );
}
