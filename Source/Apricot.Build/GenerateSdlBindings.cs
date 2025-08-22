using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Core.IO;
using Cake.Frosting;

namespace Apricot.Build;

[TaskName("Generate SDL3 bindings")]
[IsDependentOn(typeof(UpdateGitSubModules))]
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
