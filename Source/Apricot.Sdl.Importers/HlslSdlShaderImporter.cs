using System.Runtime.InteropServices;
using System.Text;
using Apricot.Assets;
using Apricot.Assets.Artifacts;
using Apricot.Graphics;
using Apricot.Graphics.Shaders;
using Microsoft.Extensions.Logging;
using SDL3;
using SDL3.ShaderCross;

namespace Apricot.Sdl.Importers;

/// <summary>
/// Uses SDL_shadercross to compile HLSL shader into SDL_gpu supported shader.
/// </summary>
/// <param name="logger"></param>
public unsafe class HlslSdlShaderImporter(ILogger<HlslSdlShaderImporter> logger) : IAssetsImporter, IDisposable
{
    private bool _isShaderInitialized;

    /// <inheritdoc />
    public bool SupportsAsset(Asset asset) =>
        asset.Uri.LocalPath.EndsWith(".hlsl", StringComparison.InvariantCultureIgnoreCase);

    /// <inheritdoc />
    public IEnumerable<ArtifactTarget> GetSupportedTargets(Asset asset) =>
    [
        new(null, GraphicDriver.Direct3d12, [AssetUtils.VertexTag]),
        new(null, GraphicDriver.Direct3d12, [AssetUtils.FragmentTag]),
        new(null, GraphicDriver.Metal, [AssetUtils.VertexTag]),
        new(null, GraphicDriver.Metal, [AssetUtils.FragmentTag]),
        new(null, GraphicDriver.Vulkan, [AssetUtils.VertexTag]),
        new(null, GraphicDriver.Vulkan, [AssetUtils.FragmentTag]),
        new(null, GraphicDriver.OpenGl, [AssetUtils.VertexTag]),
        new(null, GraphicDriver.OpenGl, [AssetUtils.FragmentTag]),
    ];

    /// <inheritdoc />
    public Artifact Import(Asset asset, Stream stream, ArtifactTarget target)
    {
        // todo: traverse all of them
        if (target.Tags is not [{ } stageTag])
        {
            throw new NotSupportedException("No stage tag were provided on import");
        }

        if (stageTag != AssetUtils.VertexTag && stageTag != AssetUtils.FragmentTag)
        {
            throw new InvalidOperationException($"Not supported shader stage: {stageTag}");
        }

        if (!_isShaderInitialized) InitializeShadercross();

        using var logScope = logger.BeginScope(new
        {
            Asset = asset,
            Target = target
        });
        logger.LogInformation("Importing {asset} for {target}", asset, target);

        using var sourceMemoryStream = new MemoryStream();
        stream.CopyTo(sourceMemoryStream);

        var nameBytes = Encoding.UTF8.GetBytes(asset.Name);
        var sourceBytes = sourceMemoryStream.ToArray();

        var stage = stageTag == AssetUtils.VertexTag
            ? ShaderStage.Vertex
            : ShaderStage.Fragment;
        var entryPoint = stage switch
        {
            ShaderStage.Vertex => "vert",
            ShaderStage.Fragment => "frag",
            _ => throw new NotSupportedException($"Not supported shader stage: {stage}")
        };
        var entryPointBytes = Encoding.UTF8.GetBytes(entryPoint);


        byte[] shaderCode;
        SdlShaderCross.SDL_ShaderCross_GraphicsShaderMetadata metadata;

        fixed (byte* sourcePtr = sourceBytes)
        fixed (byte* namePtr = nameBytes)
        fixed (byte* entryPointPtr = entryPointBytes)
        {
            logger.LogDebug("Building SPIR-V shader");
            var hlslInfo = new SdlShaderCross.SDL_ShaderCross_HLSL_Info()
            {
                source = sourcePtr,
                name = namePtr,
                entrypoint = entryPointPtr,
                defines = null,
                enable_debug = false,
                include_dir = null,
                props = 0,
                shader_stage = stage.ToSdlShadercross()
            };
            var spirVPtr = SdlShaderCross.SDL_ShaderCross_CompileSPIRVFromHLSL(hlslInfo, out var spirVSize);

            logger.LogDebug("Built SPIR-V shader located at {location} of size {size} bytes", spirVPtr, spirVSize);

            logger.LogDebug("Reflecting metadata of built SPIR-V shader");
            var metadataPtr = SdlShaderCross.SDL_ShaderCross_ReflectGraphicsSPIRV(
                (byte*)spirVPtr,
                spirVSize,
                0
            );
            metadata = metadataPtr[0];
            SDL.SDL_free((IntPtr)metadataPtr);

            var spirVInfo = new SdlShaderCross.SDL_ShaderCross_SPIRV_Info
            {
                bytecode = (byte*)spirVPtr,
                bytecode_size = spirVSize,
                enable_debug = false,
                entrypoint = entryPointPtr,
                name = namePtr,
                props = 0,
                shader_stage = stage.ToSdlShadercross()
            };

            switch (target.GraphicDriver)
            {
                case GraphicDriver.Vulkan:
                    shaderCode = new byte[spirVSize];
                    Marshal.Copy(spirVPtr, shaderCode, 0, (int)spirVSize);
                    SDL.SDL_free(spirVPtr);
                    break;

                case GraphicDriver.Direct3d12:
                    logger.LogDebug("Building Dx12 shader from SPIR-V");

                    var codePtr = SdlShaderCross.SDL_ShaderCross_CompileDXILFromSPIRV(spirVInfo, out var codeSize);
                    SDL.SDL_free(spirVPtr);

                    logger.LogDebug("Built shader located at {location} of size {size} bytes", codePtr, codeSize);

                    shaderCode = new byte[codeSize];
                    Marshal.Copy(codePtr, shaderCode, 0, (int)codeSize);
                    SDL.SDL_free(codePtr);
                    break;

                case GraphicDriver.Metal:
                    logger.LogDebug("Building metal shader from SPIR-V");

                    var metal = SdlShaderCross.SDL_ShaderCross_TranspileMSLFromSPIRV(spirVInfo);
                    logger.LogDebug("Built metal shader of length {len}", metal.Length);
                    SDL.SDL_free(spirVPtr);

                    shaderCode = Encoding.UTF8.GetBytes(metal);
                    break;

                default:
                    throw new NotSupportedException($"Target graphic driver is not supported: {target.GraphicDriver}");
            }
        }

        return new Artifact(asset.Id, target, new ShaderProgramDescription
        {
            Code = shaderCode,
            EntryPoint = entryPoint,
            SamplerCount = (int)metadata.num_samplers,
            Stage = stage,
            UniformBufferCount = (int)metadata.num_uniform_buffers
        });
    }

    /// <inheritdoc />
    public void Dispose() => SdlShaderCross.SDL_ShaderCross_Quit();

    private void InitializeShadercross()
    {
        logger.LogInformation("Initializing SDL_shadercross");

        if (!SdlShaderCross.SDL_ShaderCross_Init())
        {
            SdlException.ThrowFromLatest(nameof(SdlShaderCross.SDL_ShaderCross_Init));
        }

        _isShaderInitialized = true;
    }
}
