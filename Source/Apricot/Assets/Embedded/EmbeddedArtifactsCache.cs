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

    private readonly ILogger<EmbeddedArtifactsCache> _logger;
    private readonly List<(Assembly, EmbeddedArtifactManifest)> _manifests = [];

    public EmbeddedArtifactsCache(ILogger<EmbeddedArtifactsCache> logger)
    {
        _logger = logger;

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            LoadEmbeddedManifests(assembly);
        }
    }

    private void LoadEmbeddedManifests(Assembly assembly)
    {
        _logger.BeginScope(new
        {
            Assembly = assembly
        });

        _logger.LogInformation("Loading manifests from {assemblyName}", assembly.FullName);

        // note: EmbeddedArtifactManifest has nothing to do with assembly manifest resources
        var files = assembly.GetManifestResourceNames();

        foreach (var logicalName in files)
        {
            if (!logicalName.EndsWith("." + ManifestExtension)) continue;

            using var manifestStream = assembly.GetManifestResourceStream(logicalName);

            if (manifestStream is null)
            {
                _logger.LogWarning("Manifest resource stream for {logicalName} is null. Skipping.", logicalName);
                continue;
            }

            var manifest = MessagePackSerializer.Deserialize<EmbeddedArtifactManifest>(manifestStream);

            _logger.LogDebug("Adding manifest of {assetId} asset ({assetUri})", manifest.AssetId, manifest.AssetUri);

            _manifests.Add((assembly, manifest));
        }
    }

    public IEnumerable<Artifact> GetArtifacts(Asset asset)
    {
        foreach (var (assembly, manifest) in _manifests)
        {
            if (manifest.AssetId == asset.Id || manifest.AssetUri == asset.Uri)
            {
                yield return LoadArtifact(assembly, manifest.ArtifactLogicalName) with
                {
                    AssetId = asset.Id
                };
            }
        }
    }

    public void Add(Artifact artifact) =>
        throw new NotSupportedException("Embedded cache does not support adding new artifacts");

    private Artifact LoadArtifact(Assembly assembly, string logicalName)
    {
        _logger.LogInformation("Loading artifact {name} from {assemblyName}", logicalName, assembly.FullName);

        using var stream = assembly.GetManifestResourceStream(logicalName);

        if (stream is null) throw new FileNotFoundException("Artifacts embedded resource not found", logicalName);

        return DeserializeArtifact(stream);
    }

    /// <summary>
    /// Serializes artifact in <see cref="EmbeddedArtifactsCache"/> suitable format using message pack. With that it can
    /// be added to assembly manifest by external tool.
    /// </summary>
    public static byte[] SerializeArtifact(Artifact artifact)
    {
        var options = MessagePackSerializerOptions.Standard
            .WithResolver(MessagePack.Resolvers.TypelessContractlessStandardResolver.Instance);

        return MessagePackSerializer.Serialize(artifact, options);
    }

    /// <summary>
    /// Deserializes artifact that was previously serialized with <see cref="SerializeArtifact"/>.
    /// </summary>
    public static Artifact DeserializeArtifact(Stream stream)
    {
        var options = MessagePackSerializerOptions.Standard
            .WithResolver(MessagePack.Resolvers.TypelessContractlessStandardResolver.Instance);

        return MessagePackSerializer.Deserialize<Artifact>(stream, options);
    }
}
