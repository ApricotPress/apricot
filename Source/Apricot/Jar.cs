using Apricot.Assets;
using Apricot.Graphics;
using Apricot.Lifecycle;
using Apricot.Jobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apricot;

/// <summary>
/// Main class of Apricot. Holds all jam and rules application lifecycle. You can create your game by inheriting from it
/// but much easier would be to use Essentials project and use Game class and GameJar from there to speed up
/// development.
/// </summary>
public class Jar(
    IGameLoopProvider gameLoopProvider,
    IScheduler scheduler,
    IGraphics graphics,
    PreBakedAssetsImporter preBakedImporter,
    IEnumerable<IJarLifecycleListener> lifecycleListeners,
    IOptionsMonitor<JarOptions> jarOptions,
    ILogger<Jar> logger
)
{
    private readonly IJarLifecycleListener[] _lifecycleListeners = lifecycleListeners.ToArray();

    protected ILogger Logger { get; } = logger;

    /// <summary>
    /// Current state of jar indication what's going inside of it.
    /// </summary>
    public JarState State { get; private set; } = JarState.Uninitialized;

    /// <summary>
    /// Called to initialize subsystems.
    /// </summary>
    /// <seealso cref="IJarLifecycleListener.OnBeforeInitialization"/>
    /// <seealso cref="IJarLifecycleListener.OnAfterInitialization"/>
    /// <exception cref="InvalidOperationException">Thrown if Jar is already initialized.</exception>
    public void Init()
    {
        if (State != JarState.Uninitialized)
        {
            throw new InvalidOperationException("Jar is already initialized or initializing");
        }

        using var _ = logger.BeginScope(nameof(Init));

        State = JarState.Initializing;

        foreach (var listener in _lifecycleListeners)
        {
            listener.OnBeforeInitialization();
        }

        DoInitialization();

        foreach (var listener in _lifecycleListeners)
        {
            listener.OnAfterInitialization();
        }

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

        foreach (var listener in _lifecycleListeners)
        {
            listener.OnBeforeQuit();
        }

        logger.LogInformation("Disposing graphics");
        graphics.Dispose();

        logger.LogInformation("Stopping all background tasks");

        scheduler.StopBackground();
    }

    /// <summary>
    /// One application loop tick.
    /// </summary>
    /// <seealso cref="GameLoop"/>
    public virtual void Tick()
    {
        try
        {
            ExecuteGameLoop(gameLoopProvider.GetGameLoop());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unhandled exception during tick");
            Quit();
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
    /// Does actual initialization and called from <see cref="Init"/> after and before all lifecycle callbacks.
    ///
    /// By default, it adds all built-in assets, initializes graphics and starts scheduler of background jobs. 
    /// </summary>
    protected virtual void DoInitialization()
    {
        BuiltInAssets.Add(preBakedImporter);

        graphics.Initialize(jarOptions.CurrentValue.PreferredDriver, jarOptions.CurrentValue.EnableGraphicsDebug);
        scheduler.StartBackground();
    }

    private void ExecuteGameLoop(GameLoop loop)
    {
        foreach (var child in loop.ChildGameLoops)
        {
            ExecuteGameLoop(child);
        }

        loop.Handler?.Tick();
    }
}
