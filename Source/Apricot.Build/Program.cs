using System.Xml;
using System.Xml.Linq;
using Cake.CMake;
using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Frosting;
using Cake.Git;

return new CakeHost()
    .UseContext<BuildContext>()
    .Run(args);

public class BuildContext(ICakeContext context) : FrostingContext(context)
{
    public string SdlCmakeGenerator { get; } = context.Argument("sdlCmakeGenerator", "Ninja");

    public string? ShadercrossBinary { get; set; } = context.Argument<string?>("shadercrossBinary", null);
}

[TaskName("Fetch sub-SubModules")]
public sealed class UpdateGitSubModules : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var exitCode = context.StartProcess("git", new ProcessSettings
        {
            Arguments = ProcessArgumentBuilder.FromStrings(["submodule", "update", "--init", "--recursive"])
        });

        if (exitCode == 0)
        {
            context.Log.Information("Successfully updated git submodules");
        }
        else
        {
            context.Error($"Unable to update git submodules (exit code: {exitCode})");
        }
    }
}

[TaskName("Build SDL standalone")]
[IsDependentOn(typeof(UpdateGitSubModules))]
public sealed class BuildSdlStandalone : FrostingTask<BuildContext>
{
    public const string SdlPath = "External/SDL/";
    public const string BuildPath = SdlPath + "build-standalone/";

    public override void Run(BuildContext context)
    {
        context.Log.Information("Preparing to build SDL for current OS");
        var commit = context.GitLog(SdlPath, 1).First();
        context.Log.Information("SDL repository is at {0} - {1}", commit.Sha, commit.Message);

        context.CMake(new CMakeSettings
        {
            OutputPath = BuildPath,
            SourcePath = SdlPath,
            Generator = context.SdlCmakeGenerator,
            Options =
            [
                "-DCMAKE_BUILD_TYPE=Release",
                "-DSDL_SHARED=ON",
                "-DSDL_TESTS=OFF",
                "-DSDL_EXAMPLES=OFF",
                "-DCMAKE_OSX_ARCHITECTURES=\"arm64;x86_64\""
            ]
        });

        context.CMakeBuild(new CMakeBuildSettings
        {
            BinaryPath = BuildPath
        });

        if (context.IsRunningOnMacOs())
        {
            context.CopyFiles(BuildPath + "libSDL3.dylib", "Deps/osx");
        }
        else
        {
            throw new NotSupportedException("Cannot copy build artifacts of SDL3");
        }
    }
}

[TaskName("Build SDL for Emscripten")]
public sealed class BuildSdlWeb : FrostingTask<BuildContext>
{
    public const string SdlPath = "External/SDL/";
    public const string BuildPath = SdlPath + "build-web/";

    public override void Run(BuildContext context)
    {
        context.Log.Information("Preparing to build SDL for web");
        var commit = context.GitLog(SdlPath, 1).First();
        context.Log.Information("SDL repository is at {0} - {1}", commit.Sha, commit.Message);


        throw new NotImplementedException("Web build is not yet implemented");
    }
}

[TaskName("Generate SDL3 bindings")]
[IsDependentOn(typeof(UpdateGitSubModules))]
public sealed class GenerateSdlBindings : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        // todo: call c2ffi 


        context.DotNetBuild("External/SDL3-CS/GenerateBindings/GenerateBindings.csproj", new DotNetBuildSettings
        {
            Configuration = "Release"
        });

        context.DotNetRun(
            "External/SDL3-CS/GenerateBindings/GenerateBindings.csproj",
            ProcessArgumentBuilder.FromStrings(["External/SDL"])
        );
    }
}

[TaskName("Build shader cross")]
[IsDependentOn(typeof(UpdateGitSubModules))]
[IsDependentOn(typeof(BuildSdlStandalone))]
public sealed class BuildSdlShadercross : FrostingTask<BuildContext>
{
    public const string ShadercrossPath = "External/SDL_shadercross/";
    public const string BuildPath = ShadercrossPath + "build/";

    public override void Run(BuildContext context)
    {
        context.Log.Information("Preparing to build SDL_shadercross");
        var commit = context.GitLog(ShadercrossPath, 1).First();
        context.Log.Information("SDL_shadercross repository is at {0} - {1}", commit.Sha, commit.Message);

        context.CMake(new CMakeSettings
        {
            OutputPath = BuildPath,
            SourcePath = ShadercrossPath,
            Generator = context.SdlCmakeGenerator,
            Options =
            [
                "-DCMAKE_BUILD_TYPE=Release",
                "-DSDLSHADERCROSS_DXC=ON",
                "-DSDLSHADERCROSS_VENDORED=ON",
                $"-DSDL3_DIR={BuildSdlStandalone.BuildPath}"
            ]
        });

        context.CMakeBuild(new CMakeBuildSettings
        {
            BinaryPath = BuildPath
        });

        context.ShadercrossBinary = context.IsRunningOnWindows()
            ? BuildPath + "shadercross.exe"
            : BuildPath + "shadercross";
    }
}

[TaskName("Rebuild shaders and add to project")]
[IsDependentOn(typeof(BuildSdlShadercross))]
public sealed class RebuildShaderAssets : FrostingTask<BuildContext>
{
    private readonly string[] _stages = ["vertex", "fragment"];
    private const string EssentialsProjDir = "Source/Apricot.Essentials/";

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
            .FirstOrDefault(ig => (string?)ig.Attribute("Label") == "Shader Assets");

        if (itemGroup == null)
        {
            itemGroup = new XElement(ns + "ItemGroup", new XAttribute("Label", "Shader Assets"));
            projectDocument.Root!.Add(itemGroup);
        }

        itemGroup.RemoveNodes();

        foreach (var compiled in compiledShaders)
        {
            var includePath = essentialsProjectDir
                .Path
                .MakeAbsolute(context.Environment)
                .GetRelativePath(compiled.Path.MakeAbsolute(context.Environment));
            var embedded = new XElement(ns + "EmbeddedResource", new XAttribute("Include", includePath.ToString()));
            embedded.Add(new XElement(ns + "LogicalName", compiled.LogicalName));

            itemGroup.Add(embedded);
        }

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

[TaskName("Default")]
[IsDependentOn(typeof(BuildSdlStandalone))]
[IsDependentOn(typeof(GenerateSdlBindings))]
[IsDependentOn(typeof(RebuildShaderAssets))]
public sealed class BuildApricot : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetBuild("Source/Apricot");
        context.DotNetBuild("Source/Apricot.Essentials");
    }
}
