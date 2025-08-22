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
                    GameLoop.SimpleHandler(
                        Ids.EventPolling,
                        new EventPollerHandlersTickHandler(_services.GetServices<IEventPoller>().ToArray())
                    )
                ]
            ),
            new GameLoop(
                Ids.Update,
                [
                    GameLoop.SimpleHandler(
                        Ids.GenricUpdateHandlers,
                        new UpdateHandlersTickHandler(_services.GetServices<IUpdateHandler>().ToArray())
                    )
                ]
            ),
            new GameLoop(
                Ids.Render,
                [
                    GameLoop.SimpleHandler(
                        Ids.GenericRenderHandlers,
                        new DrawHandlersTickHandler(_services.GetServices<IRenderHandler>().ToArray())
                    )
                ]
            )
        ]
    );

    private class EventPollerHandlersTickHandler(IEventPoller[] handlers) : ITickHandler
    {
        public void Tick()
        {
            foreach (var updateHandler in handlers)
            {
                updateHandler.Poll();
            }
        }
    }

    private class UpdateHandlersTickHandler(IUpdateHandler[] handlers) : ITickHandler
    {
        public void Tick()
        {
            foreach (var updateHandler in handlers)
            {
                updateHandler.Update();
            }
        }
    }

    private class DrawHandlersTickHandler(IRenderHandler[] handlers) : ITickHandler
    {
        public void Tick()
        {
            foreach (var updateHandler in handlers)
            {
                updateHandler.Render();
            }
        }
    }
}
