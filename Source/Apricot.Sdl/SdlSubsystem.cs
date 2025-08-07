using Apricot.Events;
using Microsoft.Extensions.Logging;
using static SDL3.SDL;

namespace Apricot.Sdl;

// todo: check whether listeners are updated on hot reload
public class SdlSubsystem(
    ILogger<SdlSubsystem> logger,
    IEnumerable<ISdlEventListener> sdlEventListeners
) : IEventPoller, IJarLifecycleListener
{
    private readonly ISdlEventListener[] _listeners = sdlEventListeners.ToArray();

    public void OnBeforeInitialization()
    {
        logger.LogInformation("Initializing SDL");

        if (!SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO))
        {
            SdlException.ThrowFromLatest(nameof(SDL_Init));
        }

        logger.LogInformation("SDL version: {Version}", SDL_GetVersion());
        logger.LogInformation("Audio driver: {AudioDriver}", SDL_GetCurrentAudioDriver());
    }

    public void Poll()
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
