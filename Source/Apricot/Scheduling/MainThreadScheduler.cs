using System.Collections.Concurrent;

namespace Apricot.Scheduling;

/// <summary>
/// Simple implementation of <see cref="IMainThreadScheduler"/> with <see cref="ConcurrentQueue{T}"/> underneath.
/// </summary>
public class MainThreadScheduler : IMainThreadScheduler
{
    private readonly ConcurrentQueue<Action?> _queue = [];

    /// <inheritdoc />
    public void Schedule(Action? action)
    {
        _queue.Enqueue(action);
    }

    /// <inheritdoc />
    public Task ScheduleAsync(Action? action)
    {
        var taskCompletionSource = new TaskCompletionSource();
        
        _queue.Enqueue(() =>
        {
            try
            {
                action?.Invoke();
                
                taskCompletionSource.SetResult();
            }
            catch (Exception ex)
            {
                taskCompletionSource.SetException(ex);
            }
        });

        return taskCompletionSource.Task;
    }

    /// <inheritdoc />
    public bool HasPending => !_queue.IsEmpty;

    /// <inheritdoc />
    public void DoPending()
    {
        while (_queue.TryDequeue(out var act))
        {
            act?.Invoke();
        }
    }
}
