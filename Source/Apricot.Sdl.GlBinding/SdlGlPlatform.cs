using System.Runtime.InteropServices;
using Apricot.Sdl.Windows;
using Apricot.OpenGl;
using Apricot.Windows;
using SDL3;

namespace Apricot.Sdl.GlBinding;

public class SdlGlPlatform : IGlPlatform
{
    public IntPtr CreateGlContext(IWindow window)
    {
        var sdlWindow = ToSdlWindow(window);
        
        SDL.SDL_GL_SetAttribute(SDL.SDL_GLAttr.SDL_GL_CONTEXT_MAJOR_VERSION, 4);
        SDL.SDL_GL_SetAttribute(SDL.SDL_GLAttr.SDL_GL_CONTEXT_MINOR_VERSION, 1);
        SDL.SDL_GL_SetAttribute(SDL.SDL_GLAttr.SDL_GL_CONTEXT_PROFILE_MASK, 1); // SDL_GL_CONTEXT_PROFILE_CORE
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // SDL_GL_CONTEXT_FORWARD_COMPATIBLE_FLAG
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLAttr.SDL_GL_CONTEXT_FLAGS, 2);
        } 

        return SDL.SDL_GL_CreateContext(sdlWindow.Handle);
    }

    public IntPtr GetCurrentContext() => SDL.SDL_GL_GetCurrentContext();

    public void MakeCurrent(IWindow window, IntPtr context)
    {
        var sdlWindow = ToSdlWindow(window);

        if (!SDL.SDL_GL_MakeCurrent(sdlWindow.Handle, context))
        {
            SdlException.ThrowFromLatest(nameof(SDL.SDL_GL_MakeCurrent));
        }
    }

    public void DeleteContext(IntPtr context)
    {
        if (!SDL.SDL_GL_DestroyContext(context))
        {
            SdlException.ThrowFromLatest(nameof(SDL.SDL_GL_MakeCurrent));
        }
    }

    public IntPtr GetProcAddress(string proc) => SDL.SDL_GL_GetProcAddress(proc);

    public int SwapInterval
    {
        get => SDL.SDL_GL_GetSwapInterval(out var interval)
            ? interval
            : throw SdlException.GetFromLatest(nameof(SDL.SDL_GL_GetSwapInterval));
        set => SDL.SDL_GL_SetSwapInterval(value);
    }

    public void SwapBuffers(IWindow window)
    {
        var sdlWindow = ToSdlWindow(window);

        if (!SDL.SDL_GL_SwapWindow(sdlWindow.Handle))
        {
            SdlException.ThrowFromLatest(nameof(SDL.SDL_GL_SwapWindow));
        }
    }

    private static SdlWindow ToSdlWindow(IWindow window)
    {
        if (window is not SdlWindow sdlWindow)
        {
            throw new NotSupportedException("Only SDL windows are supported");
        }

        return sdlWindow;
    }
}
