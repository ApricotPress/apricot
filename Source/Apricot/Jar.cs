using Apricot.Events;
using Apricot.Jobs;
using Apricot.Windows;
using Microsoft.Extensions.Logging;

namespace Apricot;

/// <summary>
/// Main class of Apricot. Holds all jam and rules application lifecycle.
/// </summary>
public class Jar(
    ILogger<Jar> logger,
    IWindowsManager windows,
    IScheduler scheduler,
    IEnumerable<ISubsystem> subsystems,
    IServiceProvider services
)
{
    private readonly ISubsystem[] _subsystems = subsystems.ToArray();

    /// <summary>
    /// Current state of jar indication what's going inside of it.
    /// </summary>
    public JarState State { get; private set; } = JarState.Uninitialized;

    /// <summary>
    /// Called to initialize subsystems and create a main window. 
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if Jar is already initialized.</exception>
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

    /// <summary>
    /// Runs main application loop. Automatically initializes jar if it is not done before.
    /// </summary>
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
        AfterRun();
    }

    /// <summary>
    /// Called from <see cref="Run"/> right before starting actual app loop.
    /// </summary>
    public virtual void PrepareToRun()
    {
        logger.LogInformation("Running the jar now");
        State = JarState.Running;
    }

    /// <summary>
    /// Called from <see cref="Run"/> after stopping app loop.
    /// </summary>
    public virtual void AfterRun()
    {
        logger.LogInformation("Finalising jar life cycle");
        services.CastCallback<IJarLifecycleListener>(x => x.OnBeforeQuit());

        logger.LogInformation("Closing all windows");
        foreach (var window in windows.Windows)
        {
            window.Close();
        }

        logger.LogInformation("Calling quit on all subsystems");

        scheduler.StopBackground();

        foreach (var subsystem in subsystems)
        {
            subsystem.Quit();
        }
    }

    /// <summary>
    /// One application loop tick. Calls all systems <see cref="ISubsystem.BeforeFrame"/> to schedule or simply execute
    /// their main thread jobs. Then processes the queue and calls <see cref="ISubsystem.AfterFrame"/> for each
    /// subsystem.
    /// </summary>
    public virtual void Tick()
    {
        try
        {
            foreach (var subsystem in _subsystems)
            {
                subsystem.BeforeFrame();
            }

            scheduler.RunMainThread();

            foreach (var subsystem in _subsystems)
            {
                subsystem.AfterFrame();
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unhandled exception during tick");
            throw;
        }
    }

    /// <summary>
    /// Requests quit by setting <see cref="State"/> to <see cref="JarState.Exiting"/>. After that <see cref="Run"/>
    /// should stop execution of app loop.
    /// </summary>
    public void Quit()
    {
        logger.LogInformation("Quit was requested");
        State = JarState.Exiting;
    }

    /// <summary>
    /// Does actual initialization and called from <see cref="Init"/> after and before all lifecycle callbacks. Should
    /// create main window and subscribe for its closing.
    /// </summary>
    protected virtual void DoInitialization()
    {
        scheduler.StartBackground();

        foreach (var subsystem in subsystems)
        {
            subsystem.Initialize();
        }

        var mainWindow = windows.GetOrCreateDefaultWindow();
        mainWindow.OnClose += OnMainWindowClosed;
    }

    private void OnMainWindowClosed(IWindow mainWindow)
    {
        if (State != JarState.Running) return;

        mainWindow.OnClose -= OnMainWindowClosed;
        Quit();
    }
}
