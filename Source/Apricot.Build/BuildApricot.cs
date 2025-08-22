using Cake.Common.Tools.DotNet;
using Cake.Frosting;

namespace Apricot.Build;

[TaskName("Default")]
[IsDependentOn(typeof(BuildSdlStandalone))]
[IsDependentOn(typeof(GenerateSdlBindings))]
[IsDependentOn(typeof(RebuildShaderAssets))]
// ReSharper disable once UnusedType.Global
public sealed class BuildApricot : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetBuild("Source/Apricot");
        context.DotNetBuild("Source/Apricot.Essentials");
    }
}
