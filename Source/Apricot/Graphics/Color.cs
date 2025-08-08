namespace Apricot.Graphics;

/// <summary>
/// Represents... well... color! Stores 4 floats and provides simple color operations. All values should be in [0; 1]
/// range. (0, 0, 0, 0) represents black transparent color and (1, 1, 1, 1) represents opaque white. All provided
/// pre-defined colors have their alpha channel set to 1, except <see cref="Transparent"/>. 
/// </summary>
/// <param name="r">Red component of color.</param>
/// <param name="g">Green component of color.</param>
/// <param name="b">Blue component of color.</param>
/// <param name="a">Alpha channel of a color.</param>
public struct Color(float r, float g, float b, float a = 1) : IEquatable<Color>
{
    public static Color Black { get; } = new(0, 0, 0);
    public static Color White { get; } = new(1, 1, 1);
    public static Color Red { get; } = new(1, 0, 0);
    public static Color Green { get; } = new(0, 1, 0);
    public static Color Blue { get; } = new(0, 0, 1);
    public static Color Transparent { get; } = new(0, 0, 0, 0);

    public float R { get; set; } = r;

    public float G { get; set; } = g;

    public float B { get; set; } = b;

    public float A { get; set; } = a;

    public Color WithAlpha(float alpha) => new(R, G, B, alpha);

    public readonly override string ToString() => ToString("G");

    public readonly string ToString(string? format) =>
        $"[{R.ToString(format)}, {G.ToString(format)}, {B.ToString(format)}, {A.ToString(format)}]";

    public readonly override int GetHashCode() => HashCode.Combine(R, G, B, A);

    public readonly bool Equals(Color other) =>
        R.Equals(other.R) && G.Equals(other.G) && B.Equals(other.B) && A.Equals(other.A);

    public readonly override bool Equals(object? obj) => obj is Color other && Equals(other);

    public static bool operator ==(Color left, Color right) => left.Equals(right);

    public static bool operator !=(Color left, Color right) => !left.Equals(right);

    public static Color FromHsv(float hue, float saturation, float lightness, float alpha = 1f)
    {
        if (saturation == 0f)
        {
            return Black.WithAlpha(alpha);
        }

        const float oneSixth = 1f / 6;
        var hi = (int)(MathF.Floor(hue / oneSixth)) % 6;
        var f = hue / oneSixth - MathF.Floor(hue / oneSixth);

        var p = lightness * (1 - saturation);
        var q = lightness * (1 - f * saturation);
        var t = lightness * (1 - (1 - f) * saturation);

        return hi switch
        {
            0 => new Color(lightness, t, p, alpha),
            1 => new Color(q, lightness, p, alpha),
            2 => new Color(p, lightness, t, alpha),
            3 => new Color(p, q, lightness, alpha),
            4 => new Color(t, p, lightness, alpha),
            _ => new Color(lightness, p, q, alpha)
        };
    }
}
