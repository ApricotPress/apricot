using Apricot.Events;
using Apricot.Extensions;
using Apricot.Graphics;
using Apricot.Graphics.GameLoop;
using Apricot.Lifecycle.TickHandlers;
using Apricot.Timing;
using Apricot.Windows;
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
                    new GameLoop(
                        Ids.PreRender,
                        [
                            GameLoop.SimpleHandler(
                                Ids.PrepareMainWindow,
                                new PrepareRenderTickHandle(
                                    _services.GetRequiredService<IGraphics>(),
                                    _services.GetRequiredService<IWindowsManager>()
                                )
                            ),
                            GameLoop.SimpleHandler(
                                Ids.ImGuiBeginLayout,
                                _services.GetRequiredService<ImGuiWrapper>().MainWindowRenderer?.BeginLayout
                            )
                        ]
                    ),
                    GameLoop.SimpleHandler(
                        Ids.GenericDrawHandlers,
                        new DrawHandlersTickHandler(_services.GetServices<IDrawHandler>().ToArray())
                    ),
                    new GameLoop(
                        Ids.PresentGraphics,
                        [
                            GameLoop.SimpleHandler(
                                Ids.ImGuiEndLayout,
                                _services.GetRequiredService<ImGuiWrapper>().MainWindowRenderer?.EndLayout
                            )
                        ],
                        new PresentGraphicsTickHandle(
                            _services.GetRequiredService<IGraphics>()
                        )
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

    private class DrawHandlersTickHandler(IDrawHandler[] handlers) : ITickHandler
    {
        public void Tick()
        {
            foreach (var updateHandler in handlers)
            {
                updateHandler.Draw();
            }
        }
    }
}
