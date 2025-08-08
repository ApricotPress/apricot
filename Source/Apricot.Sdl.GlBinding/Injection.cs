using Apricot.OpenGl;
using Microsoft.Extensions.DependencyInjection;

namespace Apricot.Sdl.GlBinding;

public static class Injection
{
    public static IServiceCollection AddSdlGlPlatform(this IServiceCollection services) => services
        .AddSingleton<IGlPlatform, SdlGlPlatform>();
}
