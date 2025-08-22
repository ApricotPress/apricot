using Apricot.Assets;
using Apricot.Configuration;
using Apricot.Graphics;
using Apricot.Jobs;
using Apricot.Lifecycle;
using Apricot.Lifecycle.TickHandlers;
using Apricot.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apricot.Essentials.Bootstrap;

/// <summary>
/// Jar that initializes <see cref="Game"/> inside of DoInitialization and calls it <see cref="Game.Render"/> and
/// <see cref="Game.Update"/> method. Game would be initialized <i>after</i> graphic device, assets and main window are
/// initialized. This means it can safely use graphic resources, other assets. This jar also would automatically clear
/// window with <see cref="Game.ClearColor"/> and call <see cref="IGraphics.Present"/> after render phase. 
/// </summary>
/// <seealso cref="Injection.AddGame"/>
/// <typeparam name="TGame">Game class to use.</typeparam>
public class GameJar<TGame>(
    IServiceProvider services,
    IGameLoopProvider gameLoopProvider,
    IWindowsManager windows,
    IScheduler scheduler,
    IGraphics graphics,
    PreBakedAssetsImporter preBakedImporter,
    IEnumerable<IJarLifecycleListener> lifecycleListeners,
    IOptionsMonitor<JarOptions> jarOptions,
    ILogger<GameJar<TGame>> logger
) : Jar(gameLoopProvider, scheduler, graphics, preBakedImporter, lifecycleListeners, jarOptions, logger),
    IUpdateHandler, IRenderHandler
    where TGame : Game
{
    protected IWindowsManager Windows { get; } = windows;

    protected IGraphics Graphics { get; } = graphics;

    protected IWindow? MainWindow { get; private set; }

    protected TGame? Game { get; private set; }

    protected override void DoInitialization()
    {
        base.DoInitialization();

        MainWindow = Windows.GetOrCreateDefaultWindow();
        Game = services.GetRequiredService<TGame>();

        MainWindow.OnClose += OnMainWindowClosed;
    }

    public override void AfterRun()
    {
        Logger.LogInformation("Closing all windows");
        foreach (var window in Windows.Windows)
        {
            window.Close();
        }

        base.AfterRun();
    }

    public virtual void Update()
    {
        if (Game is null) throw new InvalidOperationException("Jar was not initialized");

        Game.Update();
    }

    public virtual void Render()
    {
        if (MainWindow is null || Game is null) throw new InvalidOperationException("Jar was not initialized");

        Graphics.SetRenderTarget(Graphics.GetWindowRenderTarget(MainWindow), Game.ClearColor);

        Game.Render();

        Graphics.Present();
    }

    protected virtual void OnMainWindowClosed(IWindow mainWindow)
    {
        if (State != JarState.Running) return;

        mainWindow.OnClose -= OnMainWindowClosed;
        Quit();
    }
}
