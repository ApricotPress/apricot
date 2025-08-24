using Apricot.Assets.Artifacts;
using Apricot.Graphics;
using Apricot.Platform;

namespace Apricot.Tests.Assets;

[TestFixture]
public class ArtifactTargetTests
{
    [TestCase(RuntimePlatform.Linux, GraphicDriver.OpenGl)]
    [TestCase(RuntimePlatform.Windows, null)]
    [TestCase(RuntimePlatform.OSX, GraphicDriver.Direct3d12)]
    public void EmptyArtifactTargetMatchesAll(RuntimePlatform? platform, GraphicDriver? gpu)
    {
        var target = new ArtifactTarget(platform, gpu, []);
        var allTargets = new ArtifactTarget(null, null, []);

        var matches = allTargets.Matches(target);

        Assert.That(matches, Is.True);
    }

    [TestCase(RuntimePlatform.Linux, RuntimePlatform.Linux, GraphicDriver.OpenGl, ExpectedResult = true)]
    [TestCase(RuntimePlatform.Linux, RuntimePlatform.Windows, null, ExpectedResult = false)]
    [TestCase(RuntimePlatform.OSX, null, null, ExpectedResult = true)]
    public bool WithSpecificOsAndGenericGpuMatches(
        RuntimePlatform targetOs,
        RuntimePlatform? queryOs,
        GraphicDriver? queryGraphics
    )
    {
        var target = new ArtifactTarget(targetOs, null, []);
        var query = new ArtifactTarget(queryOs, queryGraphics, []);

        return target.Matches(query);
    }
}
