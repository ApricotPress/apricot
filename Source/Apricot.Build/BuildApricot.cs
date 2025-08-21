using Cake.Common.Tools.DotNet;
using Cake.Frosting;

[TaskName("Default")]
[IsDependentOn(typeof(BuildSdlStandalone))]
[IsDependentOn(typeof(GenerateSdlBindings))]
[IsDependentOn(typeof(RebuildShaderAssets))]
public sealed class BuildApricot : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetBuild("Source/Apricot");
        context.DotNetBuild("Source/Apricot.Essentials");
    }
}