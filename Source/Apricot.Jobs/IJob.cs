namespace Apricot.Jobs;

/// <summary>
/// Represents a job that can be executed. Should be scheduled by <see cref="IScheduler"/>.
/// </summary>
public interface IJob
{
    void Execute();
}

/// <summary>
/// Simple implementation to wrap action delegate into a job.
/// </summary>
/// <param name="action">Body of job.</param>
public class ActionJob(Action? action) : IJob
{
    public void Execute() => action?.Invoke();
}

/// <summary>
/// Job that does nothing.
/// </summary>
public class EmptyJob : IJob
{
    public void Execute() { }
}
