using Microsoft.Extensions.Logging;

namespace Apricot.Sample;

public partial class FpsSubsystem(ILogger<FpsSubsystem> logger) : ISubsystem
{
    private DateTime _startTime;
    private ulong _framesCount;

    public void Initialize(App app)
    {
        _startTime = DateTime.Now;
    }

    public void BeforeFrame()
    {
        _framesCount++;
        
        if (_framesCount >= 400)
        {
            var elapsedTime = (DateTime.Now - _startTime).TotalSeconds;
            LogFps(logger, (float)(_framesCount / elapsedTime));

            _framesCount = 0;
            _startTime = DateTime.Now;
        }
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Avg FPS: {Fps:0.00}")]
    public static partial void LogFps(
        ILogger logger, float fps);
}
