namespace Apricot.Jobs;

public static class SchedulerExtensions
{
    /// <summary>
    /// Schedules a job for execution constructed from <see cref="Action"/> with <see cref="ActionJob"/>.
    /// </summary>
    /// <param name="scheduler">Scheduler to use.</param>
    /// <param name="action">Job body.</param>
    /// <param name="requireMainThread">Should be runned on main thread.</param>
    /// <param name="dependsOn">Job dependencies.</param>
    /// <returns>Handle of scheduled job.</returns>
    public static JobHandle Schedule(
        this IScheduler scheduler,
        Action? action, bool requireMainThread = false,
        JobHandle[]? dependsOn = null
    ) => scheduler.Schedule(new ActionJob(action), requireMainThread);
}
