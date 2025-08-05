using Apricot.Essentials.Sandbox;
using Apricot.Lifecycle.TickHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace Apricot.Essentials;

public static class Injection
{
    public static IServiceCollection AddSandbox(this IServiceCollection services) =>
        services.AddSingleton<IUpdateHandler, SandboxUpdateHandler>();
}
