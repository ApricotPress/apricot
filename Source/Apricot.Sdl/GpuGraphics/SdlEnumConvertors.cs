using Apricot.Graphics.Textures;
using SDL3;

namespace Apricot.Sdl.Graphics;

public static class SdlEnumConvertors
{
    public static SDL.SDL_GPUTextureFormat ToSdl(this TextureFormat format) => format switch
    {
        TextureFormat.R8G8B8A8 => SDL.SDL_GPUTextureFormat.SDL_GPU_TEXTUREFORMAT_R8G8B8A8_UNORM,
        TextureFormat.R8 => SDL.SDL_GPUTextureFormat.SDL_GPU_TEXTUREFORMAT_R8_UNORM,
        TextureFormat.R8G8 => SDL.SDL_GPUTextureFormat.SDL_GPU_TEXTUREFORMAT_R8G8_UNORM,
        TextureFormat.Depth24Stencil8 => SDL.SDL_GPUTextureFormat.SDL_GPU_TEXTUREFORMAT_D24_UNORM_S8_UINT,
        TextureFormat.Depth32Stencil8 => SDL.SDL_GPUTextureFormat.SDL_GPU_TEXTUREFORMAT_D32_FLOAT_S8_UINT,
        TextureFormat.Depth16 => SDL.SDL_GPUTextureFormat.SDL_GPU_TEXTUREFORMAT_D16_UNORM,
        TextureFormat.Depth24 => SDL.SDL_GPUTextureFormat.SDL_GPU_TEXTUREFORMAT_D24_UNORM,
        TextureFormat.Depth32 => SDL.SDL_GPUTextureFormat.SDL_GPU_TEXTUREFORMAT_D32_FLOAT,
        _ => throw new NotSupportedException($"{format} is not supported")
    };

    public static SDL.SDL_GPUTextureUsageFlags ToSdl(this TextureUsage usage)
    {
        SDL.SDL_GPUTextureUsageFlags flags = default;

        if (usage.HasFlag(TextureUsage.Sampling))
        {
            flags |= SDL.SDL_GPUTextureUsageFlags.SDL_GPU_TEXTUREUSAGE_SAMPLER;
        }

        if (usage.HasFlag(TextureUsage.ColorTarget))
        {
            flags |= SDL.SDL_GPUTextureUsageFlags.SDL_GPU_TEXTUREUSAGE_COLOR_TARGET;
        }

        if (usage.HasFlag(TextureUsage.DepthStencilTarget))
        {
            flags |= SDL.SDL_GPUTextureUsageFlags.SDL_GPU_TEXTUREUSAGE_DEPTH_STENCIL_TARGET;
        }

        return flags;
    }
}
