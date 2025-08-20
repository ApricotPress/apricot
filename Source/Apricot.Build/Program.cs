using Cake.CMake;
using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Common.Tools.DotNet.Run;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Frosting;
using Cake.Git;

return new CakeHost()
    .UseContext<BuildContext>()
    .Run(args);

public class BuildContext(ICakeContext context) : FrostingContext(context)
{
    public string SdlCmakeGenerator { get; } = context.Argument("sdlCmakeGenerator", "Ninja");
}

[TaskName("Update Git SubModules")]
public sealed class UpdateGitSubModules : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var exitCode = context.StartProcess("git", new ProcessSettings()
        {
            Arguments = ProcessArgumentBuilder.FromStrings(["submodule", "update"])
        });

        if (exitCode == 0)
        {
            context.Log.Information("Successfully updated git submodules");
        }
        else
        {
            context.Error($"Unable to update git submodules (exit code: {exitCode})");
        }
    }
}

[TaskName("Build SDL standalone")]
public sealed class BuildSdlStandalone : FrostingTask<BuildContext>
{
    public const string SdlPath = "External/SDL/";
    public const string BuildPath = SdlPath + "build-standalone/";

    public override void Run(BuildContext context)
    {
        context.Log.Information("Preparing to build SDL for current OS");
        var commit = context.GitLog(SdlPath, 1).First();
        context.Log.Information("SDL repository is at {0} - {1}", commit.Sha, commit.Message);

        context.CMake(new CMakeSettings
        {
            OutputPath = BuildPath,
            SourcePath = SdlPath,
            Generator = context.SdlCmakeGenerator,
            Options =
            [
                "-DCMAKE_BUILD_TYPE=Release",
                "-DSDL_SHARED=ON",
                "-DSDL_TESTS=OFF",
                "-DSDL_EXAMPLES=OFF",
                "-DCMAKE_OSX_ARCHITECTURES=\"arm64;x86_64\""
            ]
        });

        context.CMakeBuild(new CMakeBuildSettings
        {
            BinaryPath = BuildPath
        });

        if (context.IsRunningOnMacOs())
        {
            context.CopyFiles(BuildPath + "libSDL3.dylib", "Deps/osx");
        }
        else
        {
            throw new NotSupportedException("Cannot copy build artifacts of SDL3");
        }
    }
}

[TaskName("Build SDL for Emscripten")]
public sealed class BuildSdlWeb : FrostingTask<BuildContext>
{
    public const string SdlPath = "External/SDL/";
    public const string BuildPath = SdlPath + "build-web/";

    public override void Run(BuildContext context)
    {
        context.Log.Information("Preparing to build SDL for web");
        var commit = context.GitLog(SdlPath, 1).First();
        context.Log.Information("SDL repository is at {0} - {1}", commit.Sha, commit.Message);


        throw new NotImplementedException("Web build is not yet implemented");
    }
}

[TaskName("Generate SDL3 bindings")]
public sealed class GenerateSdlBindings : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        // todo: call c2ffi 


        context.DotNetBuild("External/SDL3-CS/GenerateBindings/GenerateBindings.csproj", new DotNetBuildSettings
        {
            Configuration = "Release"
        });

        context.DotNetRun(
            "External/SDL3-CS/GenerateBindings/GenerateBindings.csproj",
            ProcessArgumentBuilder.FromStrings(["External/SDL"])
        );
    }
}

[TaskName("Default")]
[IsDependentOn(typeof(BuildSdlStandalone))]
[IsDependentOn(typeof(GenerateSdlBindings))]
public sealed class BuildApricot : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetBuild("Source/Apricot");
        context.DotNetBuild("Source/Apricot.Essentials");
    }
}
