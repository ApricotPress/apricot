using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Apricot.Scheduling;

public class Scheduler(ILogger<Scheduler> logger) : IScheduler
{
    private readonly ConcurrentQueue<JobHandle> _jobQueue = new();
    private readonly ConcurrentQueue<JobHandle> _runningQueue = new();
    private readonly Dictionary<JobGroupId, JobHandle> _groupHandles = new();

    public JobHandle Schedule(IJob job, JobGroupId groupDependency) => Schedule(job, GetGroupHandle(groupDependency));

    public JobHandle Schedule(IJob job, JobHandle? dependency = null)
    {
        var handle = new JobHandle(job, dependency);

        Debug.Assert(!CheckCycles(handle, []), "Job has cycles");

        _jobQueue.Enqueue(handle);

        return handle;
    }

    public async Task RunScheduledAsync(CancellationToken cts = default)
    {
        while (_jobQueue.TryDequeue(out var handle))
        {
            _ = ExecuteHandleAsync(handle, cts)
                .ContinueWith(static _ => { }, TaskScheduler.Current);

            _runningQueue.Enqueue(handle);
        }

        while (_runningQueue.TryDequeue(out var handle))
        {
            await handle.AsTask();
        }
    }

    private static async Task ExecuteHandleAsync(JobHandle handle, CancellationToken cts)
    {
        try
        {
            if (handle.Dependency is not null)
                await handle.Dependency.AsTask().ConfigureAwait(false);

            await handle.Job.ExecuteAsync().ConfigureAwait(false);
            handle.CompletionSource.SetResult(true);
        }
        catch (Exception ex)
        {
            handle.CompletionSource.SetException(ex);
        }
    }

    private JobHandle GetGroupHandle(JobGroupId id)
    {
        return _groupHandles.TryGetValue(id, out var handle)
            ? handle
            : _groupHandles[id] = Schedule(new EmptyJob());
        ;
    }

    private static bool CheckCycles(JobHandle handle, HashSet<JobHandle> visited)
    {
        if (!visited.Add(handle))
            return true;

        if (handle.Dependency is not null)
        {
            if (CheckCycles(handle.Dependency, visited))
            {
                return true;
            }
        }

        visited.Remove(handle);
        return false;
    }
}
