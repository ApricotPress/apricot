using Microsoft.Extensions.DependencyInjection;

namespace Apricot.Essentials.Fps;

public static class FpsCounterInjection
{
    public static IServiceCollection AddStupidFpsCounter(this IServiceCollection services) => 
        services.AddSingleton<ISubsystem, StupidFpsCounterSubsystem>();
}
