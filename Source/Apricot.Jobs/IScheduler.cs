namespace Apricot.Jobs;

/// <summary>
/// Scheduler of jobs that has main and background workers and manages their dependencies.
/// </summary>
public interface IScheduler
{
    /// <summary>
    /// Schedules a job for execution on worker. If it has dependencies, it would be executed after corresponding job.
    /// handles' jobs are completed. 
    /// </summary>
    /// <param name="job">Job to schedule.</param>
    /// <param name="requireMainThread">Whether it should be run on main thread worker.</param>
    /// <param name="dependsOn">Handles of jobs it depends on.</param>
    /// <returns>Job handle that represents scheduled task.</returns>
    JobHandle Schedule(IJob job, bool requireMainThread = false, JobHandle[]? dependsOn = null);

    /// <summary>
    /// Runs all pending jobs until there are no other jobs or cancellation was requested.
    /// </summary>
    /// <param name="cancellationToken">
    /// Cancellation token to request cancellation before jobs are done executing.
    /// </param>
    void RunMainThread(CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts all background workers. They would constantly try to execute jobs.
    /// </summary>
    void StartBackground();

    /// <summary>
    /// Stops all background workers gracefully. Halting is not implemented.
    /// </summary>
    void StopBackground();
}
