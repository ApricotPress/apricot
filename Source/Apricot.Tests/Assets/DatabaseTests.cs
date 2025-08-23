using Apricot.Assets;
using Apricot.Assets.Artifacts;
using Apricot.Assets.Importing;
using Apricot.Assets.Models;
using Apricot.Platform;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Apricot.Tests.Assets;

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
        var id1 = _assetsDatabase.GetAssetId(new Uri(assetName));
        var id2 = _assetsDatabase.GetAssetId(new Uri(assetName));

        Assert.That(id1, Is.EqualTo(id2));
    }
}
