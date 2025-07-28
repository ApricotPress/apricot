using Apricot;
using Apricot.Scheduling;
using Apricot.Sdl;
using Apricot.Sdl.Windows;
using Apricot.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddLogging();
builder.Services.AddSingleton<App>();

// move all that to sdl proj
builder.Services.AddSingleton<ISubsystem, SdlSubsystem>();
builder.Services.AddSingleton<IScheduler, DefaultScheduler>();
builder.Services.AddSingleton<SdlWindowsManager>()
    .AddSingleton<IWindowsManager>(s => s.GetRequiredService<SdlWindowsManager>())
    .AddSingleton<ISdlEventListener>(s => s.GetRequiredService<SdlWindowsManager>());

builder.Services.AddHostedService<AppRunner>();

builder.Configuration.AddJsonFile("gameSettings.json", true, true);
builder.Configuration.AddEnvironmentVariables("APRICOT_");
builder.Configuration.AddCommandLine(args);

builder.Services.Configure<DefaultWindowOptions>(builder.Configuration.GetSection(nameof(DefaultWindowOptions)));

var host = builder.Build();
var app = host.Services.GetRequiredService<App>();

_ = host.RunAsync();

app.Run();

class AppRunner(App app) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken)
    {
        app.Quit();
        
        return Task.CompletedTask;
    }
}
