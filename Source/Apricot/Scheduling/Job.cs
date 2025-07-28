namespace Apricot.Scheduling;

public interface IJob
{
    Task ExecuteAsync();
}

public struct EmptyJob : IJob
{
    public Task ExecuteAsync() => Task.CompletedTask;
}
 
public struct ActionJob(Action action) : IJob
{
    public Task ExecuteAsync()
    {
        action();
        return Task.CompletedTask;
    }
}
