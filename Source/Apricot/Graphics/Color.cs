namespace Apricot.Graphics;

public struct Color(float r, float g, float b, float a = 1) : IEquatable<Color>
{
    public static Color Black { get; } = new(0, 0, 0);
    public static Color White { get; } = new(1, 1, 1);
    public static Color Red { get; } = new(1, 0, 0);
    public static Color Green { get; } = new(0, 1, 0);
    public static Color Blue { get; } = new(0, 0, 1);

    public float R { get; set; } = r;

    public float G { get; set; } = g;

    public float B { get; set; } = b;

    public float A { get; set; } = a;

    public readonly override string ToString() => ToString("G");

    public readonly string ToString(string? format) =>
        $"[{R.ToString(format)}, {G.ToString(format)}, {B.ToString(format)}, {A.ToString(format)}]";

    public readonly override int GetHashCode() => HashCode.Combine(R, G, B, A);

    public readonly bool Equals(Color other) =>
        R.Equals(other.R) && G.Equals(other.G) && B.Equals(other.B) && A.Equals(other.A);

    public readonly override bool Equals(object? obj) => obj is Color other && Equals(other);

    public static bool operator ==(Color left, Color right) => left.Equals(right);

    public static bool operator !=(Color left, Color right) => !left.Equals(right);
}
