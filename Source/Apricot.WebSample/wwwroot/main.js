import { dotnet } from './_framework/dotnet.js'
// dotnet.withEnvironmentVariable("MONO_LOG_LEVEL", "debug");
// dotnet.withEnvironmentVariable("MONO_LOG_MASK", "all");


const { setModuleImports, runMain } = await dotnet
    .withDiagnosticTracing(true)
    .withApplicationArgumentsFromQuery()
    .create();


setModuleImports('main.js', {
    setMainLoop: (cb) => {
        console.log("Setting main loop");
        dotnet.instance.Module.setMainLoop(cb)
        console.log('Main loop set');
    },
});

dotnet.instance.Module.canvas = document.getElementById("canvas");

await runMain();
