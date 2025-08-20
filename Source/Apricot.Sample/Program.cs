using Apricot;
using Apricot.Essentials;
using Apricot.Essentials.Sandbox;
using Apricot.Sdl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder
    .Services
    .AddSdl()
    .AddSdlGpuGraphics()
    // .AddSdlGlPlatform()
    // .AddOpenGl()
    .AddGame<SandboxGame>(addHostedQuit: true, builder.Configuration);

var resources = typeof(Jar)
    .Assembly
    .GetManifestResourceNames();

Console.WriteLine($"Loaded resources: {string.Join(", ", resources)}");

builder.Configuration
    .AddJsonFile("gameSettings.json", true, true)
    .AddEnvironmentVariables("APRICOT_")
    .AddCommandLine(args);

var host = builder.Build();
var jar = host.Services.GetRequiredService<Jar>();

_ = host.RunAsync();

jar.Run();
