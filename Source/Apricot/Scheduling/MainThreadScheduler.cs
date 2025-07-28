using System.Collections.Concurrent;

namespace Apricot.Scheduling;

public class MainThreadScheduler : IMainThreadScheduler
{
    private readonly ConcurrentQueue<Action> _queue = [];

    public void Schedule(Action action)
    {
        _queue.Enqueue(action);
    }

    public Task ScheduleAsync(Action action)
    {
        var taskCompletionSource = new TaskCompletionSource();
        
        _queue.Enqueue(() =>
        {
            try
            {
                action();
                
                taskCompletionSource.SetResult();
            }
            catch (Exception ex)
            {
                taskCompletionSource.SetException(ex);
            }
        });

        return taskCompletionSource.Task;
    }

    public bool HasPending => !_queue.IsEmpty;

    public void DoPending()
    {
        while (_queue.TryDequeue(out var act))
        {
            act.Invoke();
        }
    }
}
