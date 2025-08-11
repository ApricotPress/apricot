using Apricot.Graphics.Commands;
using Apricot.Graphics.Textures;
using Apricot.Graphics.Vertices;
using Apricot.Utils;
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

    public static SDL.SDL_GPUVertexElementFormat ToSdl(this VertexElementFormat format, bool normalized) =>
        (format, normalized) switch
        {
            (VertexElementFormat.Single, _) => SDL.SDL_GPUVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_FLOAT,
            (VertexElementFormat.Vector2, _) => SDL.SDL_GPUVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_FLOAT2,
            (VertexElementFormat.Vector3, _) => SDL.SDL_GPUVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_FLOAT3,
            (VertexElementFormat.Vector4, _) => SDL.SDL_GPUVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_FLOAT4,
            (VertexElementFormat.Byte4, false) => SDL.SDL_GPUVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_BYTE4,
            (VertexElementFormat.Byte4, true) => SDL.SDL_GPUVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_BYTE4_NORM,
            (VertexElementFormat.UByte4, false) => SDL.SDL_GPUVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_UBYTE4,
            (VertexElementFormat.UByte4, true) =>
                SDL.SDL_GPUVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_UBYTE4_NORM,
            (VertexElementFormat.Short2, false) => SDL.SDL_GPUVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_SHORT2,
            (VertexElementFormat.Short2, true) =>
                SDL.SDL_GPUVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_SHORT2_NORM,
            (VertexElementFormat.UShort2, false) => SDL.SDL_GPUVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_USHORT2,
            (VertexElementFormat.UShort2, true) => SDL.SDL_GPUVertexElementFormat
                .SDL_GPU_VERTEXELEMENTFORMAT_USHORT2_NORM,
            (VertexElementFormat.Short4, false) => SDL.SDL_GPUVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_SHORT4,
            (VertexElementFormat.Short4, true) =>
                SDL.SDL_GPUVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_SHORT4_NORM,
            (VertexElementFormat.UShort4, false) => SDL.SDL_GPUVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_USHORT4,
            (VertexElementFormat.UShort4, true) => SDL.SDL_GPUVertexElementFormat
                .SDL_GPU_VERTEXELEMENTFORMAT_USHORT4_NORM,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };

    public static SDL.SDL_GPUCompareOp ToSdl(this DepthCompare depthCompare) => depthCompare switch
    {
        DepthCompare.Always => SDL.SDL_GPUCompareOp.SDL_GPU_COMPAREOP_ALWAYS,
        DepthCompare.Never => SDL.SDL_GPUCompareOp.SDL_GPU_COMPAREOP_NEVER,
        DepthCompare.Less => SDL.SDL_GPUCompareOp.SDL_GPU_COMPAREOP_LESS,
        DepthCompare.Equal => SDL.SDL_GPUCompareOp.SDL_GPU_COMPAREOP_EQUAL,
        DepthCompare.LessOrEqual => SDL.SDL_GPUCompareOp.SDL_GPU_COMPAREOP_LESS_OR_EQUAL,
        DepthCompare.Greater => SDL.SDL_GPUCompareOp.SDL_GPU_COMPAREOP_GREATER,
        DepthCompare.NotEqual => SDL.SDL_GPUCompareOp.SDL_GPU_COMPAREOP_NOT_EQUAL,
        DepthCompare.GreaterOrEqual => SDL.SDL_GPUCompareOp.SDL_GPU_COMPAREOP_GREATER_OR_EQUAL,
        _ => throw new ArgumentOutOfRangeException(nameof(depthCompare), depthCompare, null)
    };

    public static SDL.SDL_GPUCullMode ToSdl(this CullMode cullMode) => cullMode switch
    {
        CullMode.None => SDL.SDL_GPUCullMode.SDL_GPU_CULLMODE_NONE,
        CullMode.Front => SDL.SDL_GPUCullMode.SDL_GPU_CULLMODE_FRONT,
        CullMode.Back => SDL.SDL_GPUCullMode.SDL_GPU_CULLMODE_BACK,
        _ => throw new ArgumentOutOfRangeException(nameof(cullMode), cullMode, null)
    };

    public static SDL.SDL_GPUColorTargetBlendState ToSdl(this BlendMode blend)
    {
        return new SDL.SDL_GPUColorTargetBlendState
        {
            enable_blend = true,
            src_color_blendfactor = GetFactor(blend.ColorSource),
            dst_color_blendfactor = GetFactor(blend.ColorDestination),
            color_blend_op = GetOp(blend.ColorOperation),
            src_alpha_blendfactor = GetFactor(blend.AlphaSource),
            dst_alpha_blendfactor = GetFactor(blend.AlphaDestination),
            alpha_blend_op = GetOp(blend.AlphaOperation),
            color_write_mask = GetFlags(blend.Mask)
        };

        // todo: refactor
        SDL.SDL_GPUBlendFactor GetFactor(BlendFactor factor) => factor switch
        {
            BlendFactor.Zero => SDL.SDL_GPUBlendFactor.SDL_GPU_BLENDFACTOR_ZERO,
            BlendFactor.One => SDL.SDL_GPUBlendFactor.SDL_GPU_BLENDFACTOR_ONE,
            BlendFactor.SrcColor => SDL.SDL_GPUBlendFactor.SDL_GPU_BLENDFACTOR_SRC_COLOR,
            BlendFactor.OneMinusSrcColor => SDL.SDL_GPUBlendFactor.SDL_GPU_BLENDFACTOR_ONE_MINUS_SRC_COLOR,
            BlendFactor.DstColor => SDL.SDL_GPUBlendFactor.SDL_GPU_BLENDFACTOR_DST_COLOR,
            BlendFactor.OneMinusDstColor => SDL.SDL_GPUBlendFactor.SDL_GPU_BLENDFACTOR_ONE_MINUS_DST_COLOR,
            BlendFactor.SrcAlpha => SDL.SDL_GPUBlendFactor.SDL_GPU_BLENDFACTOR_SRC_ALPHA,
            BlendFactor.OneMinusSrcAlpha => SDL.SDL_GPUBlendFactor.SDL_GPU_BLENDFACTOR_ONE_MINUS_SRC_ALPHA,
            BlendFactor.DstAlpha => SDL.SDL_GPUBlendFactor.SDL_GPU_BLENDFACTOR_DST_ALPHA,
            BlendFactor.OneMinusDstAlpha => SDL.SDL_GPUBlendFactor.SDL_GPU_BLENDFACTOR_ONE_MINUS_DST_ALPHA,
            BlendFactor.ConstantColor => SDL.SDL_GPUBlendFactor.SDL_GPU_BLENDFACTOR_CONSTANT_COLOR,
            BlendFactor.OneMinusConstantColor => SDL.SDL_GPUBlendFactor.SDL_GPU_BLENDFACTOR_ONE_MINUS_CONSTANT_COLOR,
            BlendFactor.SrcAlphaSaturate => SDL.SDL_GPUBlendFactor.SDL_GPU_BLENDFACTOR_SRC_ALPHA_SATURATE,
            _ => throw new NotImplementedException()
        };

        SDL.SDL_GPUBlendOp GetOp(BlendOp op) => op switch
        {
            BlendOp.Add => SDL.SDL_GPUBlendOp.SDL_GPU_BLENDOP_ADD,
            BlendOp.Subtract => SDL.SDL_GPUBlendOp.SDL_GPU_BLENDOP_SUBTRACT,
            BlendOp.ReverseSubtract => SDL.SDL_GPUBlendOp.SDL_GPU_BLENDOP_REVERSE_SUBTRACT,
            BlendOp.Min => SDL.SDL_GPUBlendOp.SDL_GPU_BLENDOP_MIN,
            BlendOp.Max => SDL.SDL_GPUBlendOp.SDL_GPU_BLENDOP_MAX,
            _ => throw new NotImplementedException()
        };

        SDL.SDL_GPUColorComponentFlags GetFlags(BlendMask mask)
        {
            SDL.SDL_GPUColorComponentFlags flags = default;
            if (mask.Has(BlendMask.Red)) flags |= SDL.SDL_GPUColorComponentFlags.SDL_GPU_COLORCOMPONENT_R;
            if (mask.Has(BlendMask.Green)) flags |= SDL.SDL_GPUColorComponentFlags.SDL_GPU_COLORCOMPONENT_G;
            if (mask.Has(BlendMask.Blue)) flags |= SDL.SDL_GPUColorComponentFlags.SDL_GPU_COLORCOMPONENT_B;
            if (mask.Has(BlendMask.Alpha)) flags |= SDL.SDL_GPUColorComponentFlags.SDL_GPU_COLORCOMPONENT_A;
            return flags;
        }
    }
}
