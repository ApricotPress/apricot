namespace Apricot.Graphics.Commands;

public struct BlendMode : IEquatable<BlendMode>
{
    public BlendOp ColorOperation { get; set; }
    public BlendFactor ColorSource { get; set; }
    public BlendFactor ColorDestination { get; set; }
    public BlendOp AlphaOperation { get; set; }
    public BlendFactor AlphaSource { get; set; }
    public BlendFactor AlphaDestination { get; set; }
    public BlendMask Mask { get; set; }

    public BlendMode(BlendOp operation, BlendFactor source, BlendFactor destination)
    {
        ColorOperation = AlphaOperation = operation;
        ColorSource = AlphaSource = source;
        ColorDestination = AlphaDestination = destination;
        Mask = BlendMask.RGBA;
    }

    public BlendMode(
        BlendOp colorOperation,
        BlendFactor colorSource,
        BlendFactor colorDestination,
        BlendOp alphaOperation,
        BlendFactor alphaSource,
        BlendFactor alphaDestination,
        BlendMask mask
    )
    {
        ColorOperation = colorOperation;
        ColorSource = colorSource;
        ColorDestination = colorDestination;
        AlphaOperation = alphaOperation;
        AlphaSource = alphaSource;
        AlphaDestination = alphaDestination;
        Mask = mask;
    }

    public static readonly BlendMode Premultiply = new(BlendOp.Add, BlendFactor.One, BlendFactor.OneMinusSrcAlpha);

    public static readonly BlendMode Add = new(BlendOp.Add, BlendFactor.One, BlendFactor.DstAlpha);

    public static readonly BlendMode Subtract = new(BlendOp.ReverseSubtract, BlendFactor.One, BlendFactor.One);

    public static readonly BlendMode Multiply = new(BlendOp.Add, BlendFactor.DstColor, BlendFactor.OneMinusSrcAlpha);

    public static readonly BlendMode Screen = new(BlendOp.Add, BlendFactor.One, BlendFactor.OneMinusSrcColor);

    public static bool operator ==(BlendMode a, BlendMode b) =>
        a.ColorOperation == b.ColorOperation &&
        a.ColorSource == b.ColorSource &&
        a.ColorDestination == b.ColorDestination &&
        a.AlphaOperation == b.AlphaOperation &&
        a.AlphaSource == b.AlphaSource &&
        a.AlphaDestination == b.AlphaDestination &&
        a.Mask == b.Mask;

    public readonly override bool Equals(object? obj) => obj is BlendMode mode && this == mode;

    public static bool operator !=(BlendMode a, BlendMode b) => !(a == b);

    public readonly bool Equals(BlendMode other) => this == other;

    public readonly override int GetHashCode() => HashCode.Combine(
        ColorOperation,
        ColorSource,
        ColorDestination,
        AlphaOperation,
        AlphaSource,
        AlphaDestination,
        Mask
    );
}
