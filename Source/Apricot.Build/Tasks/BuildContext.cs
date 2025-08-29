using Cake.Core;
using Cake.Frosting;
using JetBrains.Annotations;

namespace Apricot.Build.Tasks;

[UsedImplicitly]
public class BuildContext(ICakeContext context) : FrostingContext(context);
