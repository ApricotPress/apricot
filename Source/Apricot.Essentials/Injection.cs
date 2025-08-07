using Apricot.Lifecycle.TickHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace Apricot.Essentials;

public static class Injection
{
    public static IServiceCollection AddSandbox(this IServiceCollection services) => services
        .AddSingleton<Sandbox.Sandbox>()
        .AddSingleton<IUpdateHandler>(s => s.GetRequiredService<Sandbox.Sandbox>())
        .AddSingleton<IDrawHandler>(s => s.GetRequiredService<Sandbox.Sandbox>());
}
