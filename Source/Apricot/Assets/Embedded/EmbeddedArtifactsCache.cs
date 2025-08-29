using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using Apricot.Assets.Artifacts;
using MessagePack;
using Microsoft.Extensions.Logging;

namespace Apricot.Assets.Embedded;

/// <summary>
/// This cache loads artifact embedded into assemblies, using .NET embedded resources mechanism. By convention cache
/// first would look for <see cref="EmbeddedArtifactManifest">manfiests</see> that hold info about artifact logical
/// name. Both manifests and artifacts are expected to be serialized using Message Pack. <br/>
/// <br/>
/// To locate manifest file it is expected to use <see cref="ManifestExtension"/> as file extension in its logical name. <br/>
/// <br/>
/// If save options are provided, it is possible to add assets to project file and cache would use in-memory cache to
/// avoid assembly reload to keep newly added artifacts. Although this functionality exists it is recommended not to
/// use it. It was added for baking artifacts from CI\CD and not for use in shipped application as it obviously would
/// not have access to project files.
/// </summary>
public class EmbeddedArtifactsCache : IArtifactsCache
{
    private const string EmbeddedItemGroupLabel = "Shader Artifacts";
    private const string ManifestExtension = "eman";

    private readonly MessagePackSerializerOptions _messagePackSerializerOptions = MessagePackSerializerOptions.Standard
        .WithResolver(MessagePack.Resolvers.TypelessContractlessStandardResolver.Instance);

    private readonly Assembly _assembly;
    private readonly SaveOptions? _saveOptions;
    private readonly ILogger<EmbeddedArtifactsCache> _logger;
    private readonly List<EmbeddedArtifactManifest> _manifests = [];
    private readonly Dictionary<string, Artifact> _artifactsMemoryCache = [];

    public EmbeddedArtifactsCache(Assembly assembly, SaveOptions? saveOptions, ILogger<EmbeddedArtifactsCache> logger)
    {
        _assembly = assembly;
        _saveOptions = saveOptions;
        _logger = logger;

        LoadEmbeddedManifests(assembly);
    }

    /// <inheritdoc />
    public IEnumerable<Artifact> GetArtifacts(Asset asset)
    {
        foreach (var manifest in _manifests)
        {
            if (manifest.AssetId == asset.Id || manifest.AssetUri == asset.Uri)
            {
                yield return LoadArtifact(manifest.ArtifactLogicalName) with
                {
                    AssetId = asset.Id
                };
            }
        }
    }

    /// <inheritdoc />
    public void Add(Asset asset, Artifact artifact)
    {
        if (_saveOptions is null)
        {
            throw new InvalidOperationException(
                "To support saving of embedded artifacts provide save options when creating cache"
            );
        }

        if (!File.Exists(_saveOptions.ProjectFilePath))
        {
            throw new InvalidOperationException($"No project file found at path: {_saveOptions.ProjectFilePath}");
        }

        // SaveEmbeddedArtifactFile would also add manifest to _manifests and artifact to in-memory cache
        var artifactName = SaveEmbeddedArtifactFile(_saveOptions.BuiltArtifactsDirectory, asset, artifact);
        var projectDocument = XDocument.Load(_saveOptions.ProjectFilePath);
        var embeddedArtifactsNode = GetEmbeddedArtifactsNode(projectDocument);

        AddEmbedded(
            Path.GetRelativePath(
                Path.GetDirectoryName(_saveOptions.ProjectFilePath)!,
                GetArtifactFullFilePath(artifactName, _saveOptions.BuiltArtifactsDirectory)
            ),
            GetLogicalNameForNewArtifact(artifactName),
            embeddedArtifactsNode
        );

        AddEmbedded(
            Path.GetRelativePath(
                Path.GetDirectoryName(_saveOptions.ProjectFilePath)!,
                GetManifestFullFilePath(artifactName, _saveOptions.BuiltArtifactsDirectory)
            ),
            GetLogicalNameForManifest(artifactName),
            embeddedArtifactsNode
        );

        SaveProjectFile(projectDocument, _saveOptions.ProjectFilePath);
    }

    private void LoadEmbeddedManifests(Assembly assembly)
    {
        _logger.BeginScope(new
        {
            Assembly = assembly
        });

        _logger.LogDebug("Loading manifests from {assemblyName}", assembly.FullName);

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

            _manifests.Add(manifest);
        }
    }

    private string SaveEmbeddedArtifactFile(string directory, Asset asset, Artifact artifact)
    {
        var artifactName = Guid.NewGuid().ToString();

        var manifest = new EmbeddedArtifactManifest
        {
            ArtifactLogicalName = GetLogicalNameForNewArtifact(artifactName),
            AssetId = asset.Id,
            AssetUri = asset.Uri
        };

        _manifests.Add(manifest);
        _artifactsMemoryCache[manifest.ArtifactLogicalName] = artifact;

        var serializedArtifact = MessagePackSerializer.Serialize(artifact, _messagePackSerializerOptions);
        var serializedManifest = MessagePackSerializer.Serialize(manifest, _messagePackSerializerOptions);

        var artifactPath = GetArtifactFullFilePath(artifactName, directory);
        var manifestPath = GetManifestFullFilePath(artifactName, directory);

        Directory.CreateDirectory(Path.GetDirectoryName(artifactPath)!);
        Directory.CreateDirectory(Path.GetDirectoryName(manifestPath)!);

        File.WriteAllBytes(artifactPath, serializedArtifact);
        File.WriteAllBytes(manifestPath, serializedManifest);

        return artifactName;
    }

    private Artifact LoadArtifact(string logicalName)
    {
        _logger.LogInformation("Loading artifact {name} from {assemblyName}", logicalName, _assembly.FullName);

        if (_artifactsMemoryCache.TryGetValue(logicalName, out var artifact))
        {
            _logger.LogInformation("Artifact {name} was found in memory cache", logicalName);
            return artifact;
        }

        using var stream = _assembly.GetManifestResourceStream(logicalName);

        if (stream is null) throw new FileNotFoundException("Artifact embedded resource not found", logicalName);

        return MessagePackSerializer.Deserialize<Artifact>(stream, _messagePackSerializerOptions);
    }

    private static string GetLogicalNameForNewArtifact(string artifactName) => $"Artifacts/{artifactName}";

    private static string GetLogicalNameForManifest(string artifactName) =>
        $"Manifests/{artifactName}.{ManifestExtension}";

    private static string GetArtifactFullFilePath(string artifactName, string directory) =>
        Path.Join(directory, artifactName[..2], $"{artifactName}.art");

    private static string GetManifestFullFilePath(string artifactName, string directory) =>
        Path.Join(directory, artifactName[..2], $"{artifactName}.{ManifestExtension}");

    private static XElement GetEmbeddedArtifactsNode(XDocument projectDocument)
    {
        var ns = projectDocument.Root?.Name.Namespace ?? XNamespace.None;

        var itemGroup = projectDocument
            .Descendants(ns + "ItemGroup")
            .FirstOrDefault(ig => (string?)ig.Attribute("Label") == EmbeddedItemGroupLabel);

        if (itemGroup != null) return itemGroup;

        itemGroup = new XElement(ns + "ItemGroup", new XAttribute("Label", EmbeddedItemGroupLabel));
        var root = projectDocument.Root!;

        root.Add(new XComment("Auto-Generated XML node"));
        root.Add(itemGroup);

        return itemGroup;
    }

    private static void AddEmbedded(string path, string logicalName, XElement itemGroup)
    {
        var ns = itemGroup.Document?.Root?.Name.Namespace ?? XNamespace.None;
        var embedded = new XElement(
            ns + "EmbeddedResource",
            new XAttribute("Include", path),
            new XElement(ns + "LogicalName", logicalName)
        );
        itemGroup.Add(embedded);
    }

    private static void SaveProjectFile(XDocument projectDocument, string path)
    {
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "    ",
            NewLineChars = "\n",
            NewLineHandling = NewLineHandling.Entitize,
            OmitXmlDeclaration = true
        };

        using var xmlWriter = XmlWriter.Create(path, settings);
        projectDocument.Save(xmlWriter);
    }
}
