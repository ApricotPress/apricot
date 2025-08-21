using System.Xml;
using System.Xml.Linq;
using Cake.Common;
using Cake.Common.IO;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Frosting;

[TaskName("Rebuild shaders and add to project")]
[IsDependentOn(typeof(BuildSdlShadercross))]
public sealed class RebuildShaderAssets : FrostingTask<BuildContext>
{
    private readonly string[] _stages = ["vertex", "fragment"];
    private const string EssentialsProjDir = "Source/Apricot.Essentials/";
    private const string EmbeddedItemGroupLabel = "Shader Assets";

    public override void Run(BuildContext context)
    {
        var essentialsProjectDir = context.FileSystem.GetDirectory(EssentialsProjDir);
        var assetsDir = context.FileSystem.GetDirectory(EssentialsProjDir + "Assets");
        var compiledDirPath = assetsDir.Path.Combine("Compiled");
        var shaders = assetsDir.GetFiles("*.hlsl", SearchScope.Recursive);

        var compiledShaders = new List<CompiledShader>();

        foreach (var shader in shaders)
        {
            context.Log.Information($"Building {shader.Path}...");

            var shaderName = shader.Path.GetFilenameWithoutExtension().ToString();
            var relativeDir = assetsDir
                .Path
                .MakeAbsolute(context.Environment)
                .GetRelativePath(shader
                    .Path
                    .MakeAbsolute(context.Environment)
                    .GetDirectory()
                );

            foreach (var stage in _stages)
            {
                var entryPoint = stage == "vertex" ? "vert" : "frag";
                var fullCompiledDirPath = compiledDirPath
                    .Combine(relativeDir)
                    .Collapse();
                var spvShader = fullCompiledDirPath.CombineWithFilePath($"{shaderName}.{stage}.spv");
                var metalShader = fullCompiledDirPath.CombineWithFilePath($"{shaderName}.{stage}.msl");
                var dxShader = fullCompiledDirPath.CombineWithFilePath($"{shaderName}.{stage}.dxil");

                context.EnsureDirectoryExists(spvShader.GetDirectory());

                RunShadercross(context, shader.Path, entryPoint, stage, "HLSL", spvShader);
                RunShadercross(context, spvShader, entryPoint, stage, "SPIRV", metalShader);
                RunShadercross(context, spvShader, entryPoint, stage, "SPIRV", dxShader);

                compiledShaders.Add(new CompiledShader(
                    spvShader,
                    relativeDir.CombineWithFilePath(spvShader.GetFilename()).ToString()
                ));
                compiledShaders.Add(new CompiledShader(
                    metalShader,
                    relativeDir.CombineWithFilePath(metalShader.GetFilename()).ToString()
                ));
                compiledShaders.Add(new CompiledShader(
                    dxShader,
                    relativeDir.CombineWithFilePath(dxShader.GetFilename()).ToString()
                ));
            }
        }

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
            var embedded = new XElement(ns + "EmbeddedResource", new XAttribute("Include", includePath.ToString()));

            // either I don't understand how to do it properly, and I am utterly deranged
            // or whoever invented this is
            embedded.Add(new XText("\n            "));
            embedded.Add(new XElement(ns + "LogicalName", compiled.LogicalName));
            embedded.Add(new XText("\n        "));

            itemGroup.Add(new XText("\n        "));
            itemGroup.Add(embedded);
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

    private static void RunShadercross(
        BuildContext context,
        FilePath input,
        string entryPoint,
        string stage,
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
                    "-e",
                    entryPoint,
                    "-t",
                    stage,
                    "-s",
                    sourceType,
                    "-o",
                    outputPath.FullPath
                ])
            }
        );
    }

    private record struct CompiledShader(FilePath Path, string LogicalName);
}
