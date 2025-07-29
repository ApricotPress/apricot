using System.Diagnostics.CodeAnalysis;

namespace Apricot.Jobs;

/// <summary>
/// Scheduler of jobs that uses work-stealing deque.
/// </summary>
public class Scheduler : IScheduler
{
    private readonly JobWorker _mainThreadWorker;
    private readonly JobWorker[] _backgroundWorkers;
    private readonly Thread[] _backgroundThreads;

    private readonly CancellationTokenSource _backgroundTokenSource;
    private int _nextBackgroundWorker;

    /// <summary>
    /// Creates scheduler with background workers.
    /// </summary>
    /// <param name="backgroundWorkersCount">Amount of workers to creates.</param>
    /// <seealso cref="Environment.ProcessorCount"/>
    public Scheduler(int backgroundWorkersCount)
    {
        _mainThreadWorker = new JobWorker(0, false, false, this);
        _backgroundWorkers = new JobWorker[backgroundWorkersCount];
        _backgroundThreads = new Thread[backgroundWorkersCount];
        _backgroundTokenSource = new CancellationTokenSource();

        for (var i = 0; i < _backgroundWorkers.Length; i++)
        {
            var worker = new JobWorker(i + 1, true, true, this);

            _backgroundWorkers[i] = worker;
            _backgroundThreads[i] = new Thread(() => worker.Run(_backgroundTokenSource.Token))
            {
                Name = $"Scheduler worker #{i + 1}"
            };
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// Won't put handle to any queue if it has dependencies. It would be references by dependant job handles and would
    /// be enqueued once corresponding job is executed.
    /// </remarks>
    /// <seealso cref="EnqueueHandle"/>
    public JobHandle Schedule(IJob job, bool requireMainThread = false, JobHandle[]? dependsOn = null)
    {
        var handle = new JobHandle(job, requireMainThread, dependsOn);

        if (dependsOn is null || dependsOn.Length == 0) // otherwise it would be called by dependency at some point
        {
            EnqueueHandle(handle);
        }

        return handle;
    }

    /// <inheritdoc />
    public void RunMainThread(CancellationToken cancellationToken = default)
    {
        _mainThreadWorker.Run(cancellationToken);
    }

    /// <inheritdoc />
    public void StartBackground()
    {
        foreach (var workerThread in _backgroundThreads)
        {
            workerThread.Start();
        }
    }

    /// <inheritdoc />
    public void StopBackground() => _backgroundTokenSource.Cancel();

    internal bool TrySteal([MaybeNullWhen(false)] out JobHandle handle)
    {
        foreach (var worker in _backgroundWorkers)
        {
            if (worker.Queue.TrySteal(out handle))
            {
                return true;
            }
        }

        handle = null;
        return false;
    }

    internal void Finish(JobHandle handle)
    {
        foreach (var awaiter in handle.AwaitedBy)
        {
            if (awaiter.DependencyCompleted())
            {
                EnqueueHandle(awaiter);
            }
        }
    }

    private void EnqueueHandle(JobHandle handle)
    {
        if (handle.RequiresMainThread)
        {
            _mainThreadWorker.Add(handle);
        }
        else
        {
            _backgroundWorkers[_nextBackgroundWorker].Add(handle);
            _nextBackgroundWorker = (_nextBackgroundWorker + 1) % _backgroundWorkers.Length;
        }
    }
}
