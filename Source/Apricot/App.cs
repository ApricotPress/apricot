using Apricot.Scheduling;
using Apricot.Subsystems;
using Apricot.Windows;
using Microsoft.Extensions.Logging;

namespace Apricot;

public class App(
    ILogger<App> logger,
    IWindowsManager windows,
    IScheduler scheduler,
    IEnumerable<ISubsystem> subsystems,
    IServiceProvider services
)
{
    public AppState State { get; private set; } = AppState.Uninitialized;

    public void Init()
    {
        if (State != AppState.Uninitialized)
        {
            throw new InvalidOperationException("App is already initialized or initializing");
        }

        using var _ = logger.BeginScope(nameof(Init));

        State = AppState.Initializing;

        services.CastCallback<IAppLifecycleListener>(x => x.OnBeforeInitialization());

        DoInitialization();

        services.CastCallback<IAppLifecycleListener>(x => x.OnAfterInitialization());

        State = AppState.Initialized;
    }

    public void Run()
    {
        if (State != AppState.Initialized)
        {
            logger.LogInformation($"App is not initialized. Automatically initializing from {nameof(Run)}");
            Init();
        }

        State = AppState.Running;

        while (State == AppState.Running)
        {
            Tick();
        }
    }

    public virtual void Tick()
    {
        scheduler.RunScheduled();

        foreach (var subsystem in subsystems)
        {
            subsystem.Tick();
        }
    }

    protected virtual void DoInitialization()
    {
        foreach (var subsystem in subsystems)
        {
            subsystem.Initialize(this);
        }

        windows.GetOrCreateDefaultWindow();
    }
}
