using SDL3;

namespace Apricot.Sdl;

public interface ISdlEventListener
{
    void OnSdlEvent(SDL.SDL_Event evt);
}
