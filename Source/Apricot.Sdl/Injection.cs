using Apricot.Graphics;
using Apricot.Lifecycle.TickHandlers;
using Apricot.Sdl.GpuGraphics;
using Apricot.Sdl.Windows;
using Apricot.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace Apricot.Sdl;

public static class Injection
{
    public static IServiceCollection AddSdl(this IServiceCollection services) => services
        .AddSingleton<SdlWindowsManager>()
        .AddSingleton<IWindowsManager>(s => s.GetRequiredService<SdlWindowsManager>())
        .AddSingleton<ISdlEventListener>(s => s.GetRequiredService<SdlWindowsManager>())
        .AddSingleton<SdlSubsystem>()
        .AddSingleton<IJarLifecycleListener>(s => s.GetRequiredService<SdlSubsystem>())
        .AddSingleton<IEventPoller>(s => s.GetRequiredService<SdlSubsystem>());

    public static IServiceCollection AddSdlGpuGraphics(this IServiceCollection services) => services
        .AddSingleton<IGraphics, SdlGpuGraphics>();
}
