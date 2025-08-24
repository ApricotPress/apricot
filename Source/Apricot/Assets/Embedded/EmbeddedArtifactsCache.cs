using System.Reflection;
using Apricot.Assets.Artifacts;
using MessagePack;
using Microsoft.Extensions.Logging;

namespace Apricot.Assets.Embedded;

/// <summary>
/// This cache loads artifact embedded into assemblies, using .NET embedded resources mechanism. By convention cache
/// first would look for <see cref="EmbeddedArtifactManifest">manfiests</see> that hold info about artifact logical
/// name. Both manifests and artifacts are expected to be serialized using Message Pack. <br/>
/// <br/>
///
/// To locate manifest file it is expected to use <see cref="ManifestExtension"/> as file extension in its logical name.
/// </summary>
public class EmbeddedArtifactsCache : IArtifactsCache
{
    public const string ManifestExtension = "eman";
    
    private readonly IAssetDatabase _assets;
    private readonly ILogger<EmbeddedArtifactsCache> _logger;
    private readonly List<(Assembly, EmbeddedArtifactManifest)> _manifests = [];

    public EmbeddedArtifactsCache(IAssetDatabase assets, ILogger<EmbeddedArtifactsCache> logger)
    {
        _assets = assets;
        _logger = logger;

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            LoadEmbeddedManifests(assembly);
        }
    }

    /// <summary>
    /// Loads all manifests in provided assembly.
    /// </summary>
    /// <param name="assembly"></param>
    /// <exception cref="NullReferenceException"></exception>
    public void LoadEmbeddedManifests(Assembly assembly)
    {
        // not: EmbeddedArtifactManifest has nothing to do with assembly manifest resources
        var files = assembly.GetManifestResourceNames();

        foreach (var logicalName in files)
        {
            if (!logicalName.EndsWith("." + ManifestExtension)) continue;

            using var manifestStream = assembly.GetManifestResourceStream(logicalName);

            if (manifestStream is null) throw new NullReferenceException("Somehow manifest resource stream is null.");

            var manifest = MessagePackSerializer.Deserialize<EmbeddedArtifactManifest>(manifestStream);

            _manifests.Add((assembly, manifest));
        }
    }

    public IEnumerable<Artifact> GetArtifacts(Guid assetId)
    {
        foreach (var (assembly, manifest) in _manifests)
        {
            if (manifest.AssetId == assetId ||
                (manifest.AssetUri != null && _assets.GetAssetId(manifest.AssetUri) == assetId))
            {
                yield return LoadArtifact(assembly, manifest.ArtifactLogicalName) with
                {
                    AssetId = assetId
                };
            }
        }
    }

    private static Artifact LoadArtifact(Assembly assembly, string logicalName)
    {
        using var stream = assembly.GetManifestResourceStream(logicalName);

        if (stream is null) throw new FileNotFoundException("Artifacts embedded resource not found", logicalName);

        return DeserializeArtifact(stream);
    }

    public static byte[] SerializeArtifact(Artifact artifact)
    {
        var options = MessagePackSerializerOptions.Standard
            .WithResolver(MessagePack.Resolvers.TypelessContractlessStandardResolver.Instance);

        return MessagePackSerializer.Serialize(artifact, options);
    }

    public static Artifact DeserializeArtifact(Stream stream)
    {
        var options = MessagePackSerializerOptions.Standard
            .WithResolver(MessagePack.Resolvers.TypelessContractlessStandardResolver.Instance);

        return MessagePackSerializer.Deserialize<Artifact>(stream, options);
    }
}
