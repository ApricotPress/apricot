using Cake.Frosting;

namespace Apricot.Build.Tasks;

[TaskName("Default")]
[IsDependentOn(typeof(BuildSdlStandalone))]
[IsDependentOn(typeof(GenerateSdlBindings))]
[IsDependentOn(typeof(RebuildShaderAssets))]
// ReSharper disable once UnusedType.Global
public sealed class BuildApricot : FrostingTask<BuildContext>;
