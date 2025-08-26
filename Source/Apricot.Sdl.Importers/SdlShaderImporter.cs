using System.Runtime.InteropServices;
using System.Text;
using Apricot.Assets;
using Apricot.Assets.Artifacts;
using Apricot.Assets.Importing;
using Apricot.Graphics;
using Apricot.Graphics.Shaders;
using SDL3;
using SDL3.ShaderCross;

namespace Apricot.Sdl.Importers;

public unsafe class SdlShaderImporter : IAssetsImporter
{
    public bool SupportsAsset(string path) => path.EndsWith(".hlsl");

    public IEnumerable<ArtifactTarget> GetSupportedTargets(string path) =>
    [
        new(null, GraphicDriver.Direct3d12, [AssetUtils.VertexTag]),
        new(null, GraphicDriver.Direct3d12, [AssetUtils.FragmentTag]),
        new(null, GraphicDriver.Metal, [AssetUtils.VertexTag]),
        new(null, GraphicDriver.Metal, [AssetUtils.FragmentTag]),
        new(null, GraphicDriver.Vulkan, [AssetUtils.VertexTag]),
        new(null, GraphicDriver.Vulkan, [AssetUtils.FragmentTag]),
    ];

    public Artifact Import(string path, ArtifactTarget target)
    {
        if (target.Tags is not [{ } stageTag])
        {
            throw new NotSupportedException("No stage tag were provided on import");
        }

        if (stageTag != AssetUtils.VertexTag && stageTag != AssetUtils.FragmentTag)
        {
            throw new InvalidOperationException($"Not supported shader stage: {stageTag}");
        }

        var name = Path.GetFileNameWithoutExtension(path);
        var nameBytes = Encoding.UTF8.GetBytes(name);
        var sourceBytes = Encoding.UTF8.GetBytes(File.ReadAllText(path));

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
            var hlslInfo = new SdlShaderCross.SDL_ShaderCross_HLSL_Info()
            {
                source = sourcePtr,
                entrypoint = entryPointPtr,
                defines = null,
                enable_debug = false,
                include_dir = null,
                name = namePtr,
                props = 0,
                shader_stage = stage.ToSdlShadercross()
            };
            var spirVPtr = SdlShaderCross.SDL_ShaderCross_CompileSPIRVFromHLSL(hlslInfo, out var spirVSize);
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
                    var codePtr = SdlShaderCross.SDL_ShaderCross_CompileDXILFromSPIRV(spirVInfo, out var codeSize);
                    SDL.SDL_free(spirVPtr);

                    shaderCode = new byte[codeSize];
                    Marshal.Copy(codePtr, shaderCode, 0, (int)codeSize);
                    SDL.SDL_free(codePtr);
                    break;

                case GraphicDriver.Metal:
                    var metal = SdlShaderCross.SDL_ShaderCross_TranspileMSLFromSPIRV(spirVInfo);
                    Console.WriteLine(metal);
                    shaderCode = Encoding.UTF8.GetBytes(metal);
                    break;

                default:
                    throw new NotSupportedException($"Target graphic driver is not supported: {target.GraphicDriver}");
            }
        }

        return new Artifact(Guid.Empty, target, new ShaderProgramDescription
        {
            Code = shaderCode,
            EntryPoint = entryPoint,
            SamplerCount = (int)metadata.num_samplers,
            Stage = stage,
            UniformBufferCount = (int)metadata.num_uniform_buffers
        });
    }
}
