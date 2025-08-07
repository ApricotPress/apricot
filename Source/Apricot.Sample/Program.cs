using Apricot;
using Apricot.Essentials;
using Apricot.Sdl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder
    .Services
    .AddApricot<Jar>(addHostedQuit: true, builder.Configuration)
    .AddSdl()
    .AddSdlGraphics()
    .AddSandbox();

builder.Configuration
    .AddJsonFile("gameSettings.json", true, true)
    .AddEnvironmentVariables("APRICOT_")
    .AddCommandLine(args);

var host = builder.Build();
var jar = host.Services.GetRequiredService<Jar>();

_ = host.RunAsync();

jar.Run();
