using Microsoft.Extensions.DependencyInjection;

namespace Apricot.Scheduling;

public class SchedulersResolver(
    [FromKeyedServices(SchedulersResolver.FrameSchedulerName)] IScheduler frameScheduler,
    IMainThreadScheduler mainThreadScheduler
)
{
    public const string FrameSchedulerName = nameof(FrameSchedulerName);

    public IScheduler Frame { get; } = frameScheduler;
    public IMainThreadScheduler MainThread { get; } = mainThreadScheduler;
}
