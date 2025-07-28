using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using Apricot;
using Apricot.Sample;
using Apricot.Scheduling;
using Apricot.Sdl;
using Apricot.Sdl.Windows;
using Apricot.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


public sealed class DumbConsoleLoggerProvider : ILoggerProvider
{
    public sealed class JsLogger(string category) : ILogger
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Console.WriteLine($"[{category}:{logLevel}] {formatter(state, exception)}");
        }

        public bool IsEnabled(LogLevel logLevel) => true;
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null!;
    }

    public ILogger CreateLogger(string categoryName) => new JsLogger(categoryName);

    public void Dispose() { }
}

public partial class Program
{
    private static App? _app;
    private static IHost? _host;

    [JSImport("setMainLoop", "main.js")]
    static partial void SetMainLoop([JSMarshalAs<JSType.Function>] Action cb);

    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddLogging(static builder => builder.SetMinimumLevel(LogLevel.Information));
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, DumbConsoleLoggerProvider>());
        builder.Services.AddSingleton<App>();

        // move all that to sdl proj
        builder.Services.AddSingleton<ISubsystem, SdlSubsystem>();
        builder.Services.AddSingleton<ISubsystem, StupidFpsCounterSubsystem>();
        builder.Services.AddKeyedSingleton<IScheduler, Scheduler>(SchedulersResolver.FrameSchedulerName);
        builder.Services.AddSingleton<IMainThreadScheduler, MainThreadScheduler>();
        builder.Services.AddSingleton<SchedulersResolver>();
        builder.Services.AddSingleton<SdlWindowsManager>()
            .AddSingleton<IWindowsManager>(s => s.GetRequiredService<SdlWindowsManager>())
            .AddSingleton<ISdlEventListener>(s => s.GetRequiredService<SdlWindowsManager>());

        builder.Configuration.AddJsonFile("gameSettings.json", true, true);
        builder.Configuration.AddEnvironmentVariables("APRICOT_");
        builder.Configuration.AddCommandLine(args);

        builder.Services.Configure<DefaultWindowOptions>(
            builder.Configuration.GetSection(nameof(DefaultWindowOptions))
        );

        _host = builder.Build();
        _app = _host.Services.GetRequiredService<App>();

        _ = _host.RunAsync();
        _app.Init();

        SetMainLoop(MainLoop);

        Console.WriteLine("Hello, wasm");
    }

    [JSExport]
    private static void MainLoop() => _app?.Tick();
}
