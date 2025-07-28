using Apricot.Scheduling;
using Apricot.Utils;
using Apricot.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Apricot;

public static class Injection
{
    internal class HostedQuit<TApp>(TApp app) : IHostedService where TApp : App
    {
        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken)
        {
            app.Quit();

            return Task.CompletedTask;
        }
    }

    public static IServiceCollection AddApricot<TApp>(
        this IServiceCollection services,
        bool addHostedQuit = false,
        IConfiguration? rootConfiguration = null
    ) where TApp : App => services
        .AddSingleton<IMainThreadScheduler, MainThreadScheduler>()
        .AddSingleton<TApp>()
        .DoIf(addHostedQuit, s => s.AddHostedService<HostedQuit<TApp>>())
        .DoIf(rootConfiguration is not null, s => s
            .Configure<DefaultWindowOptions>(rootConfiguration!.GetSection(nameof(DefaultWindowOptions)))
        );
}
