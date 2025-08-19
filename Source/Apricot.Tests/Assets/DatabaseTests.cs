using Apricot.Platform;
using Apricot.Graphics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Apricot.Assets.Tests;

[TestFixture(typeof(InMemoryAssetsDatabase))]
public class DatabaseTests<TAssets> where TAssets : class, IAssetsDatabase
{
    private Mock<ILogger<TAssets>> _mockLogger;
    private Mock<IAssetsImporter> _mockImporter;
    private IAssetsDatabase _assetsDatabase;

    private readonly ImportSettings _defaultImportSettings = new(new ArtifactTarget(
        RuntimePlatform.Linux,
        null
    ));

    private readonly ImportSettings _allTargets = new(new ArtifactTarget(null, null));

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<TAssets>>();
        _mockImporter = new Mock<IAssetsImporter>();

        var services = new ServiceCollection();
        services.AddSingleton<ILogger<TAssets>>(_ => _mockLogger.Object);
        services.AddSingleton<IAssetsImporter>(_ => _mockImporter.Object);
        services.AddSingleton<IAssetsDatabase, TAssets>();
        _assetsDatabase = services
            .BuildServiceProvider()
            .GetRequiredService<IAssetsDatabase>();
    }

    [TestCase("test_image.png")]
    [TestCase("test_shader.hlsl")]
    public void GetAssetIdProducesSameId(string assetName)
    {
        var id1 = _assetsDatabase.Import(assetName, _defaultImportSettings);
        var id2 = _assetsDatabase.GetAssetId(assetName);

        Assert.That(id1, Is.EqualTo(id2));
    }

    [Test]
    public void ImporterCreatesArtifactForEachOs()
    {
        _mockImporter
            .Setup(i => i.SupportsAsset(It.IsAny<string>()))
            .Returns(true);
        _mockImporter
            .Setup(i => i.GetSupportedTargets(It.IsAny<string>()))
            .Returns([
                new ArtifactTarget(RuntimePlatform.Linux, null),
                new ArtifactTarget(RuntimePlatform.Windows, null)
            ]);
        _mockImporter
            .Setup(i => i.Import(It.IsAny<string>(), It.IsAny<ArtifactTarget>()))
            .Returns((string _, ArtifactTarget target) => new Artifact(
                "foo",
                target,
                [
                    target.GraphicDriver.HasValue ? (byte)target.GraphicDriver : (byte)255,
                    target.Platform.HasValue ? (byte)target.Platform : (byte)255
                ]
            ));

        var id = _assetsDatabase.Import("some_asset", _allTargets);

        var artifacts = _assetsDatabase.GetArtifacts(id);
        var artifact = _assetsDatabase.GetArtifact(
            id,
            new ArtifactTarget(RuntimePlatform.Windows, GraphicDriver.Direct3d12)
        );

        Assert.Multiple(() =>
        {
            Assert.That(artifacts, Has.Count.EqualTo(2));
            Assert.That(artifact, Is.Not.Null);
        });
    }
}
