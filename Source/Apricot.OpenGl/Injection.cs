using Apricot.Graphics;
using Microsoft.Extensions.DependencyInjection;

namespace Apricot.OpenGl;

public static class Injection
{
    public static IServiceCollection AddOpenGl(this IServiceCollection services) => services
        .AddSingleton<IGraphics, OpenGlGraphics>();
}
