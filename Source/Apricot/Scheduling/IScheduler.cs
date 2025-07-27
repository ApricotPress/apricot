namespace Apricot.Scheduling;

public interface IScheduler
{
    void ScheduleOnMainThread(Action action);

    void RunScheduled();
}
