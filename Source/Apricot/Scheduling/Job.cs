namespace Apricot.Scheduling;

public interface IJob
{
    Task ExecuteAsync();
}

public struct EmptyJob : IJob
{
    public Task ExecuteAsync() => Task.CompletedTask;
}
