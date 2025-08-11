using System.Runtime.InteropServices;

namespace Apricot.Graphics.Structs;

[StructLayout(LayoutKind.Sequential)]
public record struct Rect(int X, int Y, int Width, int Height)
{
    public Rect(int w, int h) : this(0, 0, w, h) { }
    
    public readonly override string ToString() => $"Rect<{X}, {Y}, {Width}, {Height}>";
}
