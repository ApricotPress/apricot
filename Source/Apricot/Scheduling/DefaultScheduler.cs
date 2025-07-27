using Microsoft.Extensions.Logging;

namespace Apricot.Scheduling;

public class DefaultScheduler(ILogger<DefaultScheduler> logger) : IScheduler
{
    private readonly List<Action> _scheduled = [];

    public void ScheduleOnMainThread(Action action)
    {
        logger.LogDebug("Scheduling action to main thread");
        _scheduled.Add(action); // todo: add no-closure option
    }

    public void RunScheduled()
    {
        if (_scheduled.Count == 0) return;

        var actionsCount = _scheduled.Count; // we do not want to run anything scheduled for next frame

        logger.LogDebug("Executing {ActionsCount} actions on main thread", actionsCount);


        for (var i = 0; i < actionsCount; i++)
        {
            _scheduled[i]();
        }

        // todo: optimize it to avoid moving of objects. May be ring buffer?
        _scheduled.RemoveRange(0, actionsCount);
    }
}
