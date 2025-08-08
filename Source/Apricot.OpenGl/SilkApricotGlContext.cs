using System;
using System.Diagnostics.CodeAnalysis;
using Apricot.Windows;
using Silk.NET.Core.Contexts;

namespace Apricot.OpenGl;

/// <summary>
/// OpenGL graphics implementation using Silk.NET bindings to opengl.
/// </summary>
public class SilkApricotGlContext(
    IWindow window,
    IGlPlatform glPlatform
) : IGLContext
{
    public IWindow Window { get; } = window;

    public IntPtr Handle { get; } = glPlatform.CreateGlContext(window);

    public IGLContextSource? Source => null;

    public bool IsCurrent => glPlatform.GetCurrentContext() == Handle;

    public IntPtr GetProcAddress(string proc, int? slot = null) => glPlatform.GetProcAddress(proc);

    public bool TryGetProcAddress(string proc, [UnscopedRef] out IntPtr addr, int? slot = null)
    {
        addr = 0;
        try
        {
            addr = GetProcAddress(proc, slot);
        }
        catch
        {
            // ignored
        }

        return addr != 0;
    }

    public void SwapInterval(int interval) => glPlatform.SwapInterval = interval;

    public void SwapBuffers() => glPlatform.SwapBuffers(Window);

    public void MakeCurrent() => glPlatform.MakeCurrent(Window, Handle);

    public void Clear()
    {
        // todo: what is that?
    }

    public void Dispose() => glPlatform.DeleteContext(Handle);
}
