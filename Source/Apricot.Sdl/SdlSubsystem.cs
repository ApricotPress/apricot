using Microsoft.Extensions.Logging;
using static SDL3.SDL;

namespace Apricot.Sdl;

// todo: check whether listerners are updated on hot reload
public class SdlSubsystem(ILogger<SdlSubsystem> logger, IEnumerable<ISdlEventListener> sdlEventListeners) : ISubsystem
{
    private App? _app;
    private ISdlEventListener[] _listeners = sdlEventListeners.ToArray();
    
    public void Initialize(App app)
    {
        _app = app;
        
        logger.LogInformation("Initializing SDL");
        
        if (!SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO))
        {
            SdlException.ThrowFromLatest(nameof(SDL_Init));
        }
        
        logger.LogInformation("SDL version is {Version}", SDL_GetVersion());
    }

    public void BeforeTick()
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
