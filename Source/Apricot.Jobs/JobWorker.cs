using System.Collections.Concurrent;
using Apricot.Jobs.Containers;

namespace Apricot.Jobs;

internal class JobWorker(int id, bool canSteal, bool infiniteRun, Scheduler scheduler)
{
    private readonly ConcurrentQueue<JobHandle> _incomingQueue = new();
    
    public WorkStealingDeque<JobHandle> Queue { get; } = new();

    public void Run(CancellationToken token = default)
    {
        while (!token.IsCancellationRequested)
        {
            while (_incomingQueue.TryDequeue(out var jobHandle))
            {
                Queue.PushBottom(jobHandle);
            }

            if (Queue.TryPopBottom(out var handle))
            {
                handle.Job.Execute();
                scheduler.Finish(handle);
            }
            else if (canSteal && scheduler.TrySteal(out var stolen))
            {
                stolen.Job.Execute();
                scheduler.Finish(stolen);
            }
            else if (infiniteRun)
            {
                Thread.Yield();
            }
            else
            {
                break;
            }
        }
    }

    public void Add(JobHandle handle) => _incomingQueue.Enqueue(handle);
}
