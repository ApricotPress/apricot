using System;
using System.Runtime.InteropServices.JavaScript;
using Apricot;
using Apricot.Sdl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


public sealed class DumbConsoleLoggerProvider : ILoggerProvider
{
    private sealed class DumbConsoleLogger(string category) : ILogger
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Console.WriteLine($"[{category}:{logLevel}] {formatter(state, exception)}");
        }

        public bool IsEnabled(LogLevel logLevel) => true;
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => null!;
    }

    public ILogger CreateLogger(string categoryName) => new DumbConsoleLogger(categoryName);

    public void Dispose() { }
}

public partial class Program
{
    private static Jar? _jar;
    private static IHost? _host;

    [JSImport("setMainLoop", "main.js")]
    static partial void SetMainLoop([JSMarshalAs<JSType.Function>] Action cb);

    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services
            .AddLogging(static builder => builder.SetMinimumLevel(LogLevel.Information))
            .AddSingleton<ILoggerProvider, DumbConsoleLoggerProvider>()
            .AddSdl()
            .AddApricot<Jar>(addHostedQuit: false, builder.Configuration);

        builder.Configuration.AddJsonFile("gameSettings.json", true, true);
        builder.Configuration.AddEnvironmentVariables("APRICOT_");
        builder.Configuration.AddCommandLine(args);

        _host = builder.Build();
        _jar = _host.Services.GetRequiredService<Jar>();

        _ = _host.RunAsync();
        _jar.Init();

        SetMainLoop(MainLoop);
    }

    [JSExport]
    private static void MainLoop() => _jar?.Tick();
}
