using Apricot.Subsystems;
using Microsoft.Extensions.Logging;
using static SDL3.SDL;

namespace Apricot.Sdl;

public class SdlSubsystem(ILogger<SdlSubsystem> logger) : ISubsystem
{
    private App? _app;
    
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

    public void Tick()
    {
        SDL_PollEvent(out var evt);
    }
}
