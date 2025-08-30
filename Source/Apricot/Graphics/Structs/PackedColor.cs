using System.Runtime.InteropServices;
using MessagePack;

namespace Apricot.Graphics.Structs;

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 4)]
[MessagePackObject(true)]
public struct PackedColor(byte r, byte g, byte b, byte a) : IEquatable<PackedColor>
{
    [Key("R")]
    public byte R { get; set; } = r;

    [Key("G")]
    public byte G { get; set; } = g;

    [Key("B")]
    public byte B { get; set; } = b;

    [Key("A")]
    public byte A { get; set; } = a;

    public readonly override int GetHashCode() => HashCode.Combine(R, G, B, A);

    public readonly bool Equals(PackedColor other) =>
        R.Equals(other.R) && G.Equals(other.G) && B.Equals(other.B) && A.Equals(other.A);

    public readonly override bool Equals(object? obj) => obj is Color other && Equals(other);

    public static bool operator ==(PackedColor left, PackedColor right) => left.Equals(right);

    public static bool operator !=(PackedColor left, PackedColor right) => !left.Equals(right);
}
