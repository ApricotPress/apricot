using Apricot;
using Apricot.Assets;
using Apricot.Assets.Artifacts;
using Apricot.Essentials;
using Apricot.Essentials.Sandbox;
using Apricot.Graphics;
using Apricot.Sdl;
using Apricot.Sdl.Importers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder
    .Services
    .AddSdl()
    .AddSdlImporters()
    .AddSdlGpuGraphics()
    // .AddSdlGlPlatform()
    // .AddOpenGl()
    .AddGame<SandboxGame>(addHostedQuit: true, builder.Configuration);

builder.Configuration
    .AddJsonFile("gameSettings.json", true, true)
    .AddEnvironmentVariables("APRICOT_")
    .AddCommandLine(args);

var host = builder.Build();
var jar = host.Services.GetRequiredService<Jar>();

_ = host.RunAsync();

jar.Run();
