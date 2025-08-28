using Apricot.Assets;
using Apricot.Assets.Artifacts;
using Apricot.Platform;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Apricot.Tests.Assets;

[TestFixture(typeof(InMemoryAssetDatabase))]
public class DatabaseTests<TAssets> where TAssets : class, IAssetDatabase
{
    private Mock<ILogger<TAssets>> _mockLogger;
    private Mock<IAssetsImporter> _mockImporter;
    private IAssetDatabase _assetDatabase;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<TAssets>>();
        _mockImporter = new Mock<IAssetsImporter>();

        var services = new ServiceCollection();
        services.AddSingleton<ILogger<TAssets>>(_ => _mockLogger.Object);
        services.AddSingleton<IAssetsImporter>(_ => _mockImporter.Object);
        services.AddSingleton<IAssetDatabase, TAssets>();
        _assetDatabase = services
            .BuildServiceProvider()
            .GetRequiredService<IAssetDatabase>();
    }

    [TestCase("test_image.png")]
    [TestCase("test_shader.hlsl")]
    public void GetAssetIdProducesSameId(string assetName)
    {
        var id1 = _assetDatabase.GetAssetId(new Uri(assetName));
        var id2 = _assetDatabase.GetAssetId(new Uri(assetName));

        Assert.That(id1, Is.EqualTo(id2));
    }
}
