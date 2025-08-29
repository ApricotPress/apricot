using System.Reflection;
using Apricot.Assets;
using Apricot.Assets.Artifacts;
using Apricot.Assets.Embedded;
using Apricot.Assets.InMemory;
using Apricot.Assets.Sources;
using Apricot.Sdl.Importers;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Apricot.Build.Tasks;

[TaskName("Build artifacts and embedded to essentials")]
public sealed class BuildArtifactsWithSdl : FrostingTask<BuildContext>
{
    private const string EssentialsProjectName = "Apricot.Essentials";
    private const string EssentialsProjDir = $"Source/{EssentialsProjectName}/";
    private const string EssentialsProjectPath = $"{EssentialsProjDir}/{EssentialsProjectName}.csproj";
    private const string ArtifactsBuildPath = $"{EssentialsProjDir}/Artifacts/";

    public override void Run(BuildContext context)
    {
        var essentialsAssembly = Assembly.Load(EssentialsProjectName);
        
        var serviceCollection = new ServiceCollection()
            .AddLogging()
            .AddSingleton(typeof(ILogger<>), typeof(CakeLogger<>))
            .AddSingleton<ICakeContext>(context)
            .AddSingleton<IAssetDatabase, InMemoryAssetDatabase>()
            .AddSingleton<IAssetsSource, EmbeddedAssetsSource>()
            .AddSingleton<IArtifactsDatabase, CachedArtifactsDatabase>()
            .AddSingleton<IArtifactsCache, EmbeddedArtifactsCache>(s => new EmbeddedArtifactsCache(
                essentialsAssembly,
                new SaveOptions(
                    new FilePath(EssentialsProjectPath).MakeAbsolute(context.Environment).ToString(),
                    new DirectoryPath(ArtifactsBuildPath).MakeAbsolute(context.Environment).ToString()
                ),
                s.GetRequiredService<ILogger<EmbeddedArtifactsCache>>()
            ))
            .AddSdlImporters();
        var container = serviceCollection.BuildServiceProvider();

        var assets = container.GetRequiredService<IAssetDatabase>();
        assets.BuildDatabase();
        assets.ImportAssets();
    }
}
