using Apricot.Events;
using Apricot.Scheduling;
using Apricot.Windows;
using Microsoft.Extensions.Logging;

namespace Apricot;

public class Jar(
    ILogger<Jar> logger,
    IWindowsManager windows,
    IMainThreadScheduler scheduler,
    IEnumerable<ISubsystem> subsystems,
    IServiceProvider services
)
{
    private readonly ISubsystem[] _subsystems = subsystems.ToArray();

    public JarState State { get; private set; } = JarState.Uninitialized;

    public void Init()
    {
        if (State != JarState.Uninitialized)
        {
            throw new InvalidOperationException("Jar is already initialized or initializing");
        }

        using var _ = logger.BeginScope(nameof(Init));

        State = JarState.Initializing;

        services.CastCallback<IJarLifecycleListener>(x => x.OnBeforeInitialization());

        DoInitialization();

        services.CastCallback<IJarLifecycleListener>(x => x.OnAfterInitialization());

        State = JarState.Initialized;
    }

    public virtual void Run()
    {
        if (State != JarState.Initialized)
        {
            logger.LogInformation($"Jar is not initialized. Automatically initializing from {nameof(Run)}");
            Init();
        }

        PrepareToRun();

        while (State == JarState.Running)
        {
            Tick();
        }

        logger.LogInformation("Ending game loop as State is now {State}", State);
    }

    public virtual void PrepareToRun()
    {
        logger.LogInformation("Running the jar now");
        State = JarState.Running;
    }

    public virtual void Tick()
    {
        foreach (var subsystem in _subsystems)
        {
            subsystem.BeforeFrame();
        }
        
        while (scheduler.HasPending)
        {
            scheduler.DoPending();
        }
    }

    public void Quit()
    {
        logger.BeginScope(nameof(Quit));

        logger.LogInformation("Quit was requested");
        State = JarState.Exiting;

        // todo: rethink quit lifecycle...
        services.CastCallback<IJarLifecycleListener>(x => x.OnBeforeQuit());

        scheduler.Schedule(() =>
            {
                foreach (var window in windows.Windows)
                {
                    window.Close();
                }
            }
        );

        foreach (var subsystem in subsystems)
        {
            subsystem.Quit();
        }
    }

    protected virtual void DoInitialization()
    {
        foreach (var subsystem in subsystems)
        {
            subsystem.Initialize(this);
        }

        var mainWindow = windows.GetOrCreateDefaultWindow();
        mainWindow.OnClose += OnMainWindowClosed;
    }

    private void OnMainWindowClosed(IWindow mainWindow)
    {
        mainWindow.OnClose -= OnMainWindowClosed;
        Quit();
    }
}
