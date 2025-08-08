using System;
using Apricot.Windows;

namespace Apricot.OpenGl;

public interface IGlPlatform
{
    IntPtr CreateGlContext(IWindow window);

    IntPtr GetCurrentContext();

    void MakeCurrent(IWindow window, IntPtr context);

    void DeleteContext(IntPtr context);

    IntPtr GetProcAddress(string proc);

    int SwapInterval { get; set; }

    void SwapBuffers(IWindow window);
}
