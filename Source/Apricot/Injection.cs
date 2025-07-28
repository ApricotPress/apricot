using Apricot.Scheduling;
using Apricot.Utils;
using Apricot.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Apricot;

public static class Injection
{
    internal class HostedQuit<TJar>(TJar jar) : IHostedService where TJar : Jar
    {
        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken)
        {
            jar.Quit();

            return Task.CompletedTask;
        }
    }

    public static IServiceCollection AddApricot<TJar>(
        this IServiceCollection services,
        bool addHostedQuit = false,
        IConfiguration? rootConfiguration = null
    ) where TJar : Jar => services
        .AddSingleton<IMainThreadScheduler, MainThreadScheduler>()
        .AddSingleton<TJar>()
        .DoIf(addHostedQuit, s => s.AddHostedService<HostedQuit<TJar>>())
        .DoIf(rootConfiguration is not null, s => s
            .Configure<DefaultWindowOptions>(rootConfiguration!.GetSection(nameof(DefaultWindowOptions)))
        );
}
