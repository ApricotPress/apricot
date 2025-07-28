using Microsoft.Extensions.DependencyInjection;

namespace Apricot.Sample;

public static class FpsCounterInjection
{
    public static IServiceCollection AddStupidFpsCounter(this IServiceCollection services) => 
        services.AddSingleton<ISubsystem, StupidFpsCounterSubsystem>();
}
