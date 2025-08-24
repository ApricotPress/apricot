using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using Apricot.Assets.Artifacts;
using Apricot.Assets.Embedded;
using Apricot.Build.Models;
using Apricot.Graphics;
using Apricot.Graphics.Shaders;
using Cake.Common;
using Cake.Common.IO;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Frosting;
using MessagePack;

namespace Apricot.Build.Tasks;

[TaskName("Rebuild shaders and add to project")]
[IsDependentOn(typeof(BuildSdlShadercross))]
public sealed class RebuildShaderAssets : FrostingTask<BuildContext>
{
    private readonly ShaderStage[] _stages = [ShaderStage.Vertex, ShaderStage.Fragment];
    private const string EssentialsProjectName = "Apricot.Essentials";
    private const string EssentialsProjDir = $"Source/{EssentialsProjectName}/";
    private const string EmbeddedItemGroupLabel = "Shader Artifacts";

    public override void Run(BuildContext context)
    {
        var essentialsProjectDir = context.FileSystem.GetDirectory(EssentialsProjDir);
        var assetsDir = context.FileSystem.GetDirectory(EssentialsProjDir + "Assets");
        var compiledDirPath = assetsDir.Path.Combine("Compiled");
        var shaders = assetsDir.GetFiles("*.hlsl", SearchScope.Recursive);

        var compiledShaders = CompileShaders(context, shaders, assetsDir, compiledDirPath);

        PatchProject(context, essentialsProjectDir, compiledShaders);
    }

    private List<CompiledShader> CompileShaders(
        BuildContext context,
        IEnumerable<IFile> shaders,
        IDirectory assetsDir,
        DirectoryPath compiledDirPath
    )
    {
        var compiledShaders = new List<CompiledShader>();

        foreach (var shader in shaders)
        {
            context.Log.Information($"Building {shader.Path}...");

            var shaderName = shader.Path.GetFilenameWithoutExtension().ToString();
            var relativeDir = assetsDir.Path
                .MakeAbsolute(context.Environment)
                .GetRelativePath(shader
                    .Path
                    .MakeAbsolute(context.Environment)
                    .GetDirectory()
                );
            var assetUri = new UriBuilder
            {
                Scheme = EmbeddedAssetsSource.Scheme,
                Host = null,
                Path = EssentialsProjectName + $"/{relativeDir}/{shader.Path.GetFilename()}"
            }.Uri;

            foreach (var stage in _stages)
            {
                var entryPoint = stage == ShaderStage.Vertex ? "vert" : "frag";
                var fullCompiledDirPath = compiledDirPath.Combine(relativeDir).Collapse();
                var jsonMetadataPath = fullCompiledDirPath.CombineWithFilePath($"{shaderName}.{stage}.json");
                var spvShaderPath = fullCompiledDirPath.CombineWithFilePath($"{shaderName}.{stage}.spv");
                var metalShaderPath = fullCompiledDirPath.CombineWithFilePath($"{shaderName}.{stage}.msl");
                var dxShaderPath = fullCompiledDirPath.CombineWithFilePath($"{shaderName}.{stage}.dxil");

                context.EnsureDirectoryExists(spvShaderPath.GetDirectory());

                RunShadercross(context, shader.Path, entryPoint, stage, "HLSL", jsonMetadataPath);

                using var metadataJson = context.FileSystem.GetFile(jsonMetadataPath).Open(FileMode.Open);
                var metadata = JsonSerializer.Deserialize<SdlShaderProgramInfo>(metadataJson);

                if (metadata is null) throw new InvalidOperationException("Could not parse Shader metadata");

                RunShadercross(context, shader.Path, entryPoint, stage, "HLSL", spvShaderPath);
                RunShadercross(context, spvShaderPath, entryPoint, stage, "SPIRV", metalShaderPath);
                RunShadercross(context, spvShaderPath, entryPoint, stage, "SPIRV", dxShaderPath);

                AddCompiledShader(spvShaderPath, relativeDir, GraphicDriver.Vulkan, stage, metadata, assetUri);
                AddCompiledShader(metalShaderPath, relativeDir, GraphicDriver.Metal, stage, metadata, assetUri);
                AddCompiledShader(dxShaderPath, relativeDir, GraphicDriver.Direct3d12, stage, metadata, assetUri);
            }
        }

        return compiledShaders;

        void AddCompiledShader(
            FilePath path,
            DirectoryPath relativeDir,
            GraphicDriver driver,
            ShaderStage stage,
            SdlShaderProgramInfo metadata,
            Uri uri
        ) => compiledShaders.Add(new CompiledShader(
            path,
            stage,
            driver,
            "Artifacts/" + relativeDir.CombineWithFilePath(path.GetFilename()),
            metadata,
            uri
        ));
    }

    private static void PatchProject(
        BuildContext context,
        IDirectory essentialsProjectDir,
        List<CompiledShader> compiledShaders
    )
    {
        context.Log.Information("Patching essentials project with compiled shaders");

        var projectPath = essentialsProjectDir.Path.CombineWithFilePath("Apricot.Essentials.csproj");
        var projectDocument = XDocument.Load(
            projectPath.FullPath,
            LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo
        );
        var ns = projectDocument.Root?.Name.Namespace ?? XNamespace.None;

        var itemGroup = projectDocument
            .Descendants(ns + "ItemGroup")
            .FirstOrDefault(ig => (string?)ig.Attribute("Label") == EmbeddedItemGroupLabel);

        if (itemGroup == null)
        {
            itemGroup = new XElement(ns + "ItemGroup", new XAttribute("Label", EmbeddedItemGroupLabel));
            var root = projectDocument.Root!;

            // xml formatting in c# was created by psychopaths
            root.Add(new XText("    "));
            root.Add(new XComment("Auto-Generated XML node"));
            root.Add(new XText("\n"));
            root.Add(new XText("    "));
            root.Add(itemGroup);
            root.Add(new XText("\n"));
        }

        itemGroup.RemoveNodes();

        foreach (var compiled in compiledShaders)
        {
            var includePath = essentialsProjectDir
                .Path
                .MakeAbsolute(context.Environment)
                .GetRelativePath(compiled.Path.MakeAbsolute(context.Environment));

            var manifest = new EmbeddedArtifactManifest(
                compiled.AssetUri,
                null,
                compiled.CompiledLogicalName
            );

            var manifestBytes = MessagePackSerializer.Serialize(manifest);
            var manifestPath = includePath + "." + EmbeddedArtifactsCache.ManifestExtension;
            File.WriteAllBytes(EssentialsProjDir + manifestPath, manifestBytes);

            var shaderBytes = File.ReadAllBytes(EssentialsProjDir + includePath);
            var programInfo = new ShaderProgramDescription(
                shaderBytes,
                compiled.Metadata.SamplersCount,
                compiled.Metadata.UniformBuffersCount,
                compiled.Stage,
                compiled.Stage == ShaderStage.Vertex ? "vert" : "frag"
            );
            var artifact = new Artifact(
                Guid.Empty,
                new ArtifactTarget(null, compiled.Driver, [compiled.Stage.ToString()]),
                programInfo
            );
            var artifactBytes = EmbeddedArtifactsCache.SerializeArtifact(artifact);
            var artifactPath = includePath + "." + EmbeddedArtifactsCache.ManifestExtension;
            File.WriteAllBytes(EssentialsProjDir + artifactPath, artifactBytes);

            AddEmbedded(artifactPath, compiled.CompiledLogicalName, ns, itemGroup);
            AddEmbedded(manifestPath, "Manifests/" + compiled.CompiledLogicalName + ".emanifest", ns, itemGroup);
        }

        itemGroup.Add(new XText("\n    "));

        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "    ",
            NewLineChars = "\n",
            NewLineHandling = NewLineHandling.Replace,
            OmitXmlDeclaration = true
        };

        using var xmlWriter = XmlWriter.Create(projectPath.FullPath, settings);
        projectDocument.Save(xmlWriter);
    }

    private static void AddEmbedded(string path, string logicalName, XNamespace ns, XElement itemGroup)
    {
        var embedded = new XElement(ns + "EmbeddedResource", new XAttribute("Include", path));

        // either I don't understand how to do it properly, and I am utterly deranged
        // or whoever invented this is
        embedded.Add(new XText("\n            "));
        embedded.Add(new XElement(ns + "LogicalName", logicalName));
        embedded.Add(new XText("\n        "));

        itemGroup.Add(new XText("\n        "));
        itemGroup.Add(embedded);
    }

    private static void RunShadercross(
        BuildContext context,
        FilePath input,
        string entryPoint,
        ShaderStage stage,
        string sourceType,
        FilePath outputPath
    )
    {
        context.Log.Information(
            "Cross-compiling {0} {1} stage to {2}",
            input.FullPath,
            stage,
            outputPath.FullPath
        );

        context.StartProcess(
            context.ShadercrossBinary,
            new ProcessSettings
            {
                Arguments = ProcessArgumentBuilder.FromStrings([
                    input.FullPath,
                    "-e", entryPoint,
                    "-t", stage == ShaderStage.Vertex ? "vertex" : "fragment",
                    "-s", sourceType,
                    "-o", outputPath.FullPath
                ])
            }
        );
    }

    private record struct CompiledShader(
        FilePath Path,
        ShaderStage Stage,
        GraphicDriver Driver,
        string CompiledLogicalName,
        SdlShaderProgramInfo Metadata,
        Uri AssetUri
    );
}
