namespace Apricot.Scheduling;

public interface IMainThreadScheduler
{
    void Schedule(Action? action);

    Task ScheduleAsync(Action? action);
    
    bool HasPending { get; }
    
    void DoPending();
}
