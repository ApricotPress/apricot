using System.Diagnostics;

namespace Apricot.Timing;

/// <summary>
/// Implements Apricot time mechanism using <see cref="Stopwatch"/>. 
/// </summary>
public class StopwatchTime : ITime
{
    private readonly Stopwatch _gameTime = Stopwatch.StartNew();
    private readonly Stopwatch _frameTime = new();

    /// <inheritdoc />
    public float Delta { get; private set; }
    
    /// <inheritdoc />
    public float Time => (float)_gameTime.Elapsed.TotalSeconds;

    /// <inheritdoc />
    public void Step()
    {
        Delta = (float)_frameTime.Elapsed.TotalSeconds;
        _frameTime.Restart();
    }
}
