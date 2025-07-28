using Apricot.Scheduling;
using Microsoft.Extensions.Logging;
using static SDL3.SDL;

namespace Apricot.Sdl;

// todo: check whether listerners are updated on hot reload
public class SdlSubsystem(
    SchedulersResolver schedulers,
    ILogger<SdlSubsystem> logger,
    IEnumerable<ISdlEventListener> sdlEventListeners
) : ISubsystem
{
    private readonly ISdlEventListener[] _listeners = sdlEventListeners.ToArray();
    private Action? ReadEventsAction => field ??= ReadEvents;

    public void Initialize(App app)
    {
        logger.LogInformation("Initializing SDL");

        if (!SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO))
        {
            SdlException.ThrowFromLatest(nameof(SDL_Init));
        }

        logger.LogInformation("SDL version is {Version}", SDL_GetVersion());
    }

    public void BeforeFrame() => schedulers.MainThread.Schedule(ReadEventsAction);

    private void ReadEvents()
    {
        // todo: check whether this can be de-facto while (true) loop        
        while (SDL_PollEvent(out var evt))
        {
            foreach (var listener in _listeners)
            {
                listener.OnSdlEvent(evt);
            }
        }
    }
}
