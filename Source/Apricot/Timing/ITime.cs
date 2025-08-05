namespace Apricot.Timing;

/// <summary>
/// Measures application time and delta between game frames.
/// </summary>
public interface ITime
{
    /// <summary>
    /// Time since last frame start.
    /// </summary>
    float Delta { get; }
    
    /// <summary>
    /// Time since application start.
    /// </summary>
    float Time { get; }
    
    /// <summary>
    /// Should be called as fist thing in the update loop to measure time between frames.
    /// </summary>
    void Step();
}
