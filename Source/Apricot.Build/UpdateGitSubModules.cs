using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Frosting;

namespace Apricot.Build;

[TaskName("Fetch sub-SubModules")]
public sealed class UpdateGitSubModules : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var exitCode = context.StartProcess("git", new ProcessSettings
        {
            Arguments = ProcessArgumentBuilder.FromStrings(["submodule", "update", "--init", "--recursive"])
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
