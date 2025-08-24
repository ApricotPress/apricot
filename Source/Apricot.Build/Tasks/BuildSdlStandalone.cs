using Cake.CMake;
using Cake.Common;
using Cake.Common.IO;
using Cake.Core.Diagnostics;
using Cake.Frosting;
using Cake.Git;

namespace Apricot.Build.Tasks;

[TaskName("Build SDL standalone")]
[IsDependentOn(typeof(UpdateGitSubModules))]
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
