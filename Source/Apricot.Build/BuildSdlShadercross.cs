using Cake.CMake;
using Cake.Common;
using Cake.Core.Diagnostics;
using Cake.Frosting;
using Cake.Git;

[TaskName("Build shader cross")]
[IsDependentOn(typeof(UpdateGitSubModules))]
[IsDependentOn(typeof(BuildSdlStandalone))]
public sealed class BuildSdlShadercross : FrostingTask<BuildContext>
{
    public const string ShadercrossPath = "External/SDL_shadercross/";
    public const string BuildPath = ShadercrossPath + "build/";

    public override void Run(BuildContext context)
    {
        context.Log.Information("Preparing to build SDL_shadercross");
        var commit = context.GitLog(ShadercrossPath, 1).First();
        context.Log.Information("SDL_shadercross repository is at {0} - {1}", commit.Sha, commit.Message);

        context.CMake(new CMakeSettings
        {
            OutputPath = BuildPath,
            SourcePath = ShadercrossPath,
            Generator = context.SdlCmakeGenerator,
            Options =
            [
                "-DCMAKE_BUILD_TYPE=Release",
                "-DSDLSHADERCROSS_DXC=ON",
                "-DSDLSHADERCROSS_VENDORED=ON",
                $"-DSDL3_DIR={BuildSdlStandalone.BuildPath}"
            ]
        });

        context.CMakeBuild(new CMakeBuildSettings
        {
            BinaryPath = BuildPath
        });

        context.ShadercrossBinary = context.IsRunningOnWindows()
            ? BuildPath + "shadercross.exe"
            : BuildPath + "shadercross";
    }
}