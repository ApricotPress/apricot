using SDL3;

namespace Apricot.Sdl;

public class SdlException(string methodName, string error) : Exception($"{methodName}: {error}")
{
    public static SdlException GetFromLatest(string methodName) => new(methodName, SDL.SDL_GetError());

    public static void ThrowFromLatest(string methodName) => throw GetFromLatest(methodName);
}
