using Apricot;
using Apricot.Essentials.Fps;
using Apricot.Sdl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddSdl()
    .AddStupidFpsCounter()
    .AddApricot<Jar>(addHostedQuit: true, builder.Configuration);

builder.Configuration
    .AddJsonFile("gameSettings.json", true, true)
    .AddEnvironmentVariables("APRICOT_")
    .AddCommandLine(args);

var host = builder.Build();
var jar = host.Services.GetRequiredService<Jar>();

_ = host.RunAsync();

jar.Run();
