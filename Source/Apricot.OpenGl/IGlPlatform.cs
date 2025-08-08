using System;
using Apricot.Windows;

namespace Apricot.OpenGl;

/// <summary>
/// This abstraction is used to connect OpenGL to any platform and should be added to DI container of game engine if
/// OpenGL is intended to use.<br/>
/// <br/>
/// You can use SDL implementation (in Apricot.Sdl.GlBinding) as a reference implementation.
/// </summary>
public interface IGlPlatform
{
    /// <summary>
    /// Creates OGL context for window.
    /// </summary>
    /// <returns>Opaque pointer to OpenGL context that is then used inside other IGlPlatform methods.</returns>
    IntPtr CreateGlContext(IWindow window);

    IntPtr GetCurrentContext();

    void MakeCurrent(IWindow window, IntPtr context);

    void DeleteContext(IntPtr context);

    IntPtr GetProcAddress(string proc);

    int SwapInterval { get; set; }

    void SwapBuffers(IWindow window);
}
