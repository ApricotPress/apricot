using Apricot;
using Apricot.Sample;
using Apricot.Sdl;
using Apricot.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddLogging()
    .AddSdl()
    .AddStupidFpsCounter()
    .AddApricot<App>(addHostedQuit: true, builder.Configuration);

builder.Configuration.AddJsonFile("gameSettings.json", true, true);
builder.Configuration.AddEnvironmentVariables("APRICOT_");
builder.Configuration.AddCommandLine(args);

var host = builder.Build();
var app = host.Services.GetRequiredService<App>();

_ = host.RunAsync();

app.Run();
