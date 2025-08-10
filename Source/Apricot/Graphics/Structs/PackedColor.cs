using System.Runtime.InteropServices;

namespace Apricot.Graphics.Structs;

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 4)]
public struct PackedColor(byte r, byte g, byte b, byte a) : IEquatable<PackedColor>
{
    public byte R { get; set; } = r;

    public byte G { get; set; } = g;

    public byte B { get; set; } = b;

    public byte A { get; set; } = a;

    public readonly override int GetHashCode() => HashCode.Combine(R, G, B, A);

    public readonly bool Equals(PackedColor other) =>
        R.Equals(other.R) && G.Equals(other.G) && B.Equals(other.B) && A.Equals(other.A);

    public readonly override bool Equals(object? obj) => obj is Color other && Equals(other);

    public static bool operator ==(PackedColor left, PackedColor right) => left.Equals(right);

    public static bool operator !=(PackedColor left, PackedColor right) => !left.Equals(right);
}
