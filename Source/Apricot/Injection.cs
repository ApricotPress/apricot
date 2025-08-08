using System.Diagnostics.CodeAnalysis;
using Apricot.Jobs;
using Apricot.Lifecycle;
using Apricot.Timing;
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

    /// <summary>
    /// Adds all default apricot services to collection and binds configuration if root <see cref="IConfiguration"/>
    /// provided. 
    /// </summary>
    /// <param name="services">Services collections.</param>
    /// <param name="addHostedQuit">
    /// Would add <see cref="HostedQuit{TJar}"/> whose only job is to call quit when app gracefully quitting.
    /// </param>
    /// <param name="rootConfiguration">
    /// Configuration section where all default options can be stored. Ignored if null
    /// </param>
    /// <typeparam name="TJar">Base class of jar.</typeparam>
    /// <returns>Modified service collection.</returns>
    public static IServiceCollection AddApricot<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TJar
    >(
        this IServiceCollection services,
        bool addHostedQuit = false,
        IConfiguration? rootConfiguration = null
    ) where TJar : Jar => services
        .AddSingleton<IScheduler>(s => new Scheduler(Environment.ProcessorCount - 1))
        .AddSingleton<TJar>()
        .AddSingleton<ITimeController, TimeController>()
        .AddSingleton<ITime, StopwatchTime>()
        .AddSingleton<IGameLoopProvider, DefaultGameLoopProvider>()
        .DoIf(addHostedQuit, s => s.AddHostedService<HostedQuit<TJar>>())
        .DoIf(rootConfiguration is not null, s => s
            .Configure<MainWindowOptions>(rootConfiguration!.GetSection(nameof(MainWindowOptions)))
            .Configure<JarOptions>(rootConfiguration.GetSection(nameof(JarOptions)))
        );
}
