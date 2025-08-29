using Cake.Frosting;
using JetBrains.Annotations;

namespace Apricot.Build.Tasks;

[TaskName("Default")]
[IsDependentOn(typeof(BuildArtifactsWithSdl))]
[UsedImplicitly]
public sealed class BuildApricot : FrostingTask<BuildContext>;
