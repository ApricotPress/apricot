using System.Collections.Concurrent;

namespace Apricot.Jobs;

public class JobHandle
{
    private ConcurrentBag<JobHandle>? _awaitedBy;
    private int _dependsOnCount;

    public IJob Job { get; }

    public bool RequiresMainThread { get; }

    public JobHandle[]? DependsOn { get; private set; }

    public IReadOnlyCollection<JobHandle> AwaitedBy => _awaitedBy ?? [];

    internal JobHandle(IJob job, bool onMain, JobHandle[]? dependsOn)
    {
        Job = job;
        RequiresMainThread = onMain;

        if (dependsOn is null) return;

        DependsOn = dependsOn;
        _dependsOnCount = DependsOn.Length;

        foreach (var dependency in dependsOn)
        {
            dependency._awaitedBy ??= [];
            dependency._awaitedBy.Add(this);
        }
    }

    internal bool DependencyCompleted()
    {
        var ready = Interlocked.Decrement(ref _dependsOnCount) <= 0;

        if (ready)
        {
            DependsOn = null;
        }

        return ready;
    }
}
