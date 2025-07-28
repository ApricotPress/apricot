namespace Apricot.Scheduling;

public interface IScheduler
{
    JobHandle Schedule(IJob job, JobGroupId groupDependency);

    JobHandle Schedule(IJob job, JobHandle? dependency = null);

    Task RunScheduledAsync(CancellationToken cts = default);
}
