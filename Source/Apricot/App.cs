using System.Diagnostics.CodeAnalysis;
using Apricot.Events;
using Apricot.Scheduling;
using Apricot.Windows;
using Microsoft.Extensions.Logging;

namespace Apricot;

public class App(
    ILogger<App> logger,
    IWindowsManager windows,
    SchedulersResolver schedulers,
    IEnumerable<ISubsystem> subsystems,
    IServiceProvider services
)
{
    private readonly ISubsystem[] _subsystems = subsystems.ToArray();

    [field: AllowNull, MaybeNull]
    private Func<Task> RunScheduledAction => field ??= () => schedulers.Frame.RunScheduledAsync();

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

    public virtual void Run()
    {
        if (State != AppState.Initialized)
        {
            logger.LogInformation($"App is not initialized. Automatically initializing from {nameof(Run)}");
            Init();
        }

        logger.LogInformation("Running the app now");
        State = AppState.Running;

        while (State == AppState.Running)
        {
            Tick();
        }

        logger.LogInformation("Ending game loop as State is now {State}", State);
        logger.LogInformation("Running all left tasks");

        schedulers.Frame.RunScheduledAsync().GetAwaiter().GetResult();
    }

    public virtual void Tick()
    {
        foreach (var subsystem in _subsystems)
        {
            subsystem.BeforeFrame();
        }

        _ = Task.Run(RunScheduledAction);

        while (schedulers.MainThread.HasPending)
        {
            schedulers.MainThread.DoPending();
        }
    }

    public void Quit()
    {
        logger.BeginScope(nameof(Quit));

        logger.LogInformation("Quit was requested");
        State = AppState.Exiting;

        // todo: rethink quit lifecycle...
        services.CastCallback<IAppLifecycleListener>(x => x.OnBeforeQuit());

        schedulers.MainThread.Schedule(() =>
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
