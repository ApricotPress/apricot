using Apricot.Events;
using Apricot.Lifecycle.TickHandlers;
using Apricot.Timing;
using Microsoft.Extensions.DependencyInjection;

namespace Apricot.Lifecycle;

public class DefaultGameLoopProvider : IGameLoopProvider
{
    private readonly IServiceProvider _services;
    private readonly Lazy<GameLoop> _defaultGameLoop;

    public DefaultGameLoopProvider(IServiceProvider services)
    {
        _services = services;
        _defaultGameLoop = new Lazy<GameLoop>(CreateGameLoop);
    }

    public GameLoop GetGameLoop() => _defaultGameLoop.Value;

    private GameLoop CreateGameLoop() => new(
        Ids.DefaultGameLoop,
        [
            new GameLoop(
                Ids.PreUpdate,
                [
                    GameLoop.SimpleHandler(
                        Ids.Time,
                        _services.GetRequiredService<ITimeController>()
                    ),
                    new GameLoop(
                        Ids.EventPolling,
                        GameLoopsFromServices<IEventPoller>()
                    )
                ]
            ),
            new GameLoop(
                Ids.Update,
                [
                    new GameLoop(
                        Ids.GenricUpdateHandlers,
                        GameLoopsFromServices<IUpdateHandler>()
                    )
                ]
            ),
            new GameLoop(
                Ids.Render,
                []
            )
        ]
    );

    private GameLoop[] GameLoopsFromServices<T>() where T : ITickHandler => _services
        .GetServices<T>()
        .Select(p => GameLoop.SimpleHandler(p.GetType().Name, p))
        .ToArray();
}
