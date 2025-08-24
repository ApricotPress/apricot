using Cake.Core.Diagnostics;
using Cake.Frosting;
using Cake.Git;

namespace Apricot.Build.Tasks;

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
