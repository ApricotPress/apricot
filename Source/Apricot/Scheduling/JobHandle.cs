namespace Apricot.Scheduling;

public class JobHandle(IJob job, JobHandle? dependency = null)
{
    internal JobHandle? Dependency { get; } = dependency;

    internal IJob Job { get; } = job;

    internal TaskCompletionSource<bool> CompletionSource { get; } = new();

    public Task AsTask() => CompletionSource.Task;
}
