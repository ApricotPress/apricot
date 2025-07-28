using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apricot.Essentials.Fps;

/// <summary>
/// A very tupid subsystem to print average FPS to logs.
/// </summary>
public partial class StupidFpsCounterSubsystem(
    IOptionsMonitor<FpsCounterOptions> options,
    ILogger<StupidFpsCounterSubsystem> logger
) : ISubsystem
{
    private DateTime _startTime;
    private ulong _framesCount;

    public void Initialize()
    {
        _startTime = DateTime.Now;
    }

    public void BeforeFrame()
    {
        _framesCount++;

        if (_framesCount < options.CurrentValue.MeasureFramesCount) return;

        var elapsedTime = (DateTime.Now - _startTime).TotalSeconds;
        LogFps(logger, (float)(_framesCount / elapsedTime), _framesCount);

        _framesCount = 0;
        _startTime = DateTime.Now;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Avg FPS: {Fps:0.00} in {Frames} frames")]
    public static partial void LogFps(ILogger logger, float fps, ulong frames);
}
