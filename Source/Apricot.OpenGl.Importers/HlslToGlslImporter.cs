using System.Runtime.InteropServices;
using System.Text;
using Apricot.Assets;
using Apricot.Assets.Artifacts;
using Apricot.Graphics;
using Apricot.Graphics.Shaders;
using Apricot.OpenGl.Importers.Bindings;
using Microsoft.Extensions.Logging;

namespace Apricot.OpenGl.Importers;

public unsafe class HlslToGlslImporter(ILogger<HlslToGlslImporter> logger) : IAssetsImporter
{
    public bool SupportsAsset(Asset asset) =>
        asset.Uri.LocalPath.EndsWith(".hlsl", StringComparison.InvariantCultureIgnoreCase);

    public IEnumerable<ArtifactTarget> GetSupportedTargets(Asset asset) =>
    [
        new(null, GraphicDriver.OpenGl, [AssetUtils.VertexTag]),
        new(null, GraphicDriver.OpenGl, [AssetUtils.FragmentTag]),
    ];

    public Artifact Import(Asset asset, Stream stream, ArtifactTarget target)
    {
        if (target.Tags is not [{ } stageTag])
        {
            throw new NotSupportedException("No stage tag were provided on import");
        }

        if (stageTag != AssetUtils.VertexTag && stageTag != AssetUtils.FragmentTag)
        {
            throw new InvalidOperationException($"Not supported shader stage: {stageTag}");
        }


        var stage = stageTag == AssetUtils.VertexTag
            ? ShaderStage.Vertex
            : ShaderStage.Fragment;

        var spirvCode = CreateSpirVProgram(stream, stage);
        var glslCode = SpirvToGlsl(spirvCode);
        

        return new Artifact(asset.Id, target, new ShaderProgramDescription
        {
            Code = Encoding.UTF8.GetBytes(glslCode),
            EntryPoint = "main",
            SamplerCount = 0,
            Stage = stage,
            UniformBufferCount = 0
        });
    }

    private uint[] CreateSpirVProgram(Stream stream, ShaderStage stage)
    {
        using var logScope = logger.BeginScope("Create SPIR-V program");

        byte[] sourceBytes;
        using (var sourceMemoryStream = new MemoryStream())
        {
            stream.CopyTo(sourceMemoryStream);
            sourceBytes = sourceMemoryStream.ToArray();
        }

        Glslang.Input input;

        fixed (byte* srcBytesPtr = sourceBytes)
        {
            input = new Glslang.Input
            {
                language = Glslang.SourceType.Hlsl,
                stage = stage == ShaderStage.Fragment ? Glslang.Stage.Fragment : Glslang.Stage.Vertex,
                client = Glslang.Client.Vulkan,
                client_version = Glslang.TargetClientVersion.Vulkan12,
                target_language = Glslang.TargetLanguage.Spirv,
                target_language_version = Glslang.TargetLanguageVersion.Spirv16,
                code = (char*)srcBytesPtr,
                default_version = 100,
                default_profile = Glslang.Profile.Core,
                force_default_version_and_profile = 0,
                forward_compatible = 0,
                messages = Glslang.Messages.DefaultBit,
                resource = GlslangDefaultResourceLimits.DefaultResource()
            };
        }

        var shader = Glslang.ShaderCreate(input);

        try
        {
            if (!Glslang.ShaderPreprocess(shader, input))
            {
                logger.LogError("{InfoLog}", Glslang.ShaderGetInfoLog(shader));
                logger.LogError("{DebugLog}", Glslang.ShaderGetInfoDebugLog(shader));

                throw new Exception("Error pre-processing shader");
            }

            logger.LogTrace("{PreprocessedCode}", Glslang.ShaderGetPreprocessedCode(shader));

            if (!Glslang.ShaderParse(shader, input))
            {
                logger.LogError("{InfoLog}", Glslang.ShaderGetInfoLog(shader));
                logger.LogError("{DebugLog}", Glslang.ShaderGetInfoDebugLog(shader));

                throw new Exception("Error parsing shader");
            }

            var program = Glslang.ProgramCreate();
            if (program == IntPtr.Zero) throw new Exception("glslang_program_create failed");

            try
            {
                Glslang.ProgramAddShader(program, shader);

                if (!Glslang.ProgramLink(program, Glslang.Messages.DefaultBit))
                {
                    logger.LogError("Error linking shader ro program");
                    logger.LogError("Info: {InfoLog}", Glslang.ProgramGetInfoLog(program));
                    logger.LogError("Debug: {InfoDebug}", Glslang.ProgramGetInfoDebugLog(program));

                    throw new Exception("Error linking program");
                }

                Glslang.ProgramSpirvGenerate(program, input.stage);

                var wc = Glslang.ProgramSpirvGetSize(program);
                if (wc == 0) throw new Exception("SPIR-V generation produced 0 words");

                var spirv = new uint[(int)wc];
                fixed (uint* pWords = spirv)
                {
                    Glslang.ProgramSpirvGet(program, pWords);
                }

                return spirv;
            }
            finally
            {
                Glslang.ProgramDelete(program);
            }
        }
        finally
        {
            Glslang.ShaderDelete(shader);
        }
    }

    private static unsafe string SpirvToGlsl(uint[] spirv)
    {
        fixed (uint* pWords = spirv)
        {
            Spvc.spvc_context_create(out var ctx).ThrowIfError(ctx);

            try
            {
                Spvc.spvc_context_parse_spirv(ctx, pWords, (nuint)spirv.Length, out var parsedIr);

                Spvc.spvc_context_create_compiler(
                    ctx,
                    SpvcBackend.Glsl,
                    parsedIr,
                    SpvcCaptureMode.TakeOwnership,
                    out var compiler
                ).ThrowIfError(ctx);
                Spvc.spvc_compiler_create_compiler_options(compiler, out var options).ThrowIfError(ctx);
                Spvc.spvc_compiler_options_set_uint(options, CompilerOption.GlslVersion, 410).ThrowIfError(ctx);
                Spvc.spvc_compiler_options_set_bool(options, CompilerOption.GlslEs, 0).ThrowIfError(ctx);
                Spvc.spvc_compiler_options_set_bool(options, CompilerOption.GlslEnable420PackExtension, 0)
                    .ThrowIfError(ctx);
                Spvc.spvc_compiler_options_set_bool(options, CompilerOption.GlslVulkanSemantics, 0).ThrowIfError(ctx);
                Spvc.spvc_compiler_options_set_bool(options, CompilerOption.GlslSeparateShaderObjects, 1)
                    .ThrowIfError(ctx);
                Spvc.spvc_compiler_install_compiler_options(compiler, options).ThrowIfError(ctx);

                Spvc.spvc_compiler_compile(compiler, out var source).ThrowIfError(ctx);

                return Marshal.PtrToStringUTF8(source)!;
            }
            finally
            {
                if (ctx != IntPtr.Zero)
                {
                    Spvc.spvc_context_release_allocations(ctx);
                    Spvc.spvc_context_destroy(ctx);
                }
            }
        }
    }
}
