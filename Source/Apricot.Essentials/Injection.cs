using System.Diagnostics.CodeAnalysis;
using Apricot.Assets;
using Apricot.Assets.Artifacts;
using Apricot.Assets.Embedded;
using Apricot.Assets.Sources;
using Apricot.Configuration;
using Apricot.Essentials.Bootstrap;
using Apricot.Graphics.Shaders;
using Apricot.Jobs;
using Apricot.Lifecycle;
using Apricot.Lifecycle.TickHandlers;
using Apricot.Platform;
using Apricot.Resources;
using Apricot.Timing;
using Apricot.Utils;
using Apricot.Windows;
using LiteDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Apricot.Essentials;

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
        .DoIf(typeof(TJar) != typeof(Jar), s => s.AddSingleton<Jar>(s_ => s_.GetRequiredService<TJar>()))
        .AddSingleton<IPlatformInfo, DefaultPlatformInfo>()
        .AddSingleton<ITimeController, TimeController>()
        .AddSingleton<ITime, StopwatchTime>()
        .AddSingleton<IAssetDatabase, InMemoryAssetDatabase>()
        .AddSingleton<IAssetsSource>(new FilesAssetsSource("file", "Assets"))
        .AddSingleton<IAssetsSource, EmbeddedAssetsSource>()
        .AddSingleton<IArtifactsDatabase, CachedArtifactsDatabase>()
        .AddSingleton<IArtifactsCache>(s => new LiteDbArtifactsCache(
            s.GetRequiredService<ILogger<LiteDbArtifactsCache>>(),
            new ConnectionString("Filename=artifacts.litedb"))
        )
        .AddSingleton<IArtifactsCache, EmbeddedArtifactsCache>()
        .AddSingleton<IResources, Resources.Resources>()
        .AddSingleton<IResourceFactory<ShaderProgram, Uri>, ShadersFactory>()
        .AddSingleton<IGameLoopProvider, DefaultGameLoopProvider>()
        .DoIf(addHostedQuit, s => s.AddHostedService<HostedQuit<TJar>>())
        .DoIf(rootConfiguration is not null, s => s
            // todo: make it trimming friendly
            // https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8/runtime#configuration-binding-source-generator
            .Configure<MainWindowOptions>(rootConfiguration!.GetSection(nameof(MainWindowOptions)))
            .Configure<JarOptions>(rootConfiguration.GetSection(nameof(JarOptions)))
        );

    public static IServiceCollection AddGame<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TGame
    >(
        this IServiceCollection services,
        bool addHostedQuit = false,
        IConfiguration? rootConfiguration = null
    ) where TGame : Game => services
        .AddSingleton<TGame>()
        .AddApricot<GameJar<TGame>>(addHostedQuit, rootConfiguration)
        .AddSingleton<IUpdateHandler>(s => s.GetRequiredService<GameJar<TGame>>())
        .AddSingleton<IRenderHandler>(s => s.GetRequiredService<GameJar<TGame>>());
}
