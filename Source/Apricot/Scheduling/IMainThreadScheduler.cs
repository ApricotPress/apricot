namespace Apricot.Scheduling;

/// <summary>
/// Scheduler of tasks to proceed on main thread. Does the pending jobs from <see cref="Apricot.Jar.Tick"/> each frame.
/// </summary>
public interface IMainThreadScheduler
{
    /// <summary>
    /// Schedules <see cref="Action"/> to run on main thread. Would be called this frame if called while
    /// <see cref="DoPending"/> is still running.
    /// </summary>
    /// <param name="action">Action to schedule.</param>
    void Schedule(Action? action);

    /// <summary>
    /// Does the same as <see cref="Schedule"/> but constructs awaitable <see cref="Task"/> to await if needed. Would
    /// also pass exception if threw.
    /// </summary>
    /// <param name="action">Action to schedule</param>
    /// <returns>Task of scheduled job.</returns>
    Task ScheduleAsync(Action? action);
    
    /// <summary>
    /// Indicates whether scheduled has pending work for current frame.
    /// </summary>
    bool HasPending { get; }
    
    /// <summary>
    /// Process current pending jobs.
    /// </summary>
    void DoPending();
}
