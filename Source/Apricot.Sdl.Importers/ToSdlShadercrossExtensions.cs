using Apricot.Graphics.Shaders;
using SDL3.ShaderCross;

namespace Apricot.Sdl.Importers;

public static class ToSdlShadercrossExtensions
{
    public static SdlShaderCross.SDL_ShaderCross_ShaderStage ToSdlShadercross(this ShaderStage stage) => stage switch
    {
        ShaderStage.Vertex => SdlShaderCross.SDL_ShaderCross_ShaderStage.SDL_SHADERCROSS_SHADERSTAGE_VERTEX,
        ShaderStage.Fragment => SdlShaderCross.SDL_ShaderCross_ShaderStage.SDL_SHADERCROSS_SHADERSTAGE_FRAGMENT,
        _ => throw new ArgumentOutOfRangeException(nameof(stage), stage, null)
    };
}
