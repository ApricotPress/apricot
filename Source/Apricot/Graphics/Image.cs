using System.Runtime.CompilerServices;
using Apricot.Graphics.Structs;
using MessagePack;

namespace Apricot.Graphics;

/// <summary>
/// CPU-bound image. May be used as source data for textures. 
/// </summary>
[MessagePackObject(true, AllowPrivate = true)]
public partial class Image
{
    private readonly Color[] _colors;

    public int Width { get; }

    public int Height { get; }

    /// <summary>
    /// Read-only span containing colors data.
    /// </summary>
    [IgnoreMember]
    public ReadOnlySpan<Color> Data => _colors;

    [IgnoreMember]
    public Color this[int x, int y]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] get => GetPixel(x, y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] set => SetPixel(x, y, value);
    }

    public Image(int width, int height) : this(width, height, new Color[width * height]) { }

    public Image(int width, int height, Color fill) : this(width, height, new Color[width * height])
    {
        Array.Fill(_colors, fill);
    }

    public Image(int width, int height, Color[] colors)
    {
        if (width <= 0) throw new ArgumentException($"Width should be positive: {width}", nameof(width));
        if (height <= 0) throw new ArgumentException($"Height should be positive: {height}", nameof(height));
        if (colors.Length != width * height)
        {
            throw new ArgumentException(
                $"Colors array length does not match width ({width}) and height ({height}): {colors.Length}",
                nameof(colors)
            );
        }

        _colors = colors;
        Width = width;
        Height = height;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetPixel(int x, int y, Color color)
    {
        if (x < 0 || x >= Width) throw new IndexOutOfRangeException($"X should be in range [0; {Width}): {x}");
        if (y < 0 || y >= Height) throw new IndexOutOfRangeException($"Y should be in range [0; {Height}): {y}");

        _colors[x + y * Width] = color;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Color GetPixel(int x, int y)
    {
        if (x < 0 || x >= Width) throw new IndexOutOfRangeException($"X should be in range [0; {Width}): {x}");
        if (y < 0 || y >= Height) throw new IndexOutOfRangeException($"Y should be in range [0; {Height}): {y}");

        return _colors[x + y * Width];
    }
}
