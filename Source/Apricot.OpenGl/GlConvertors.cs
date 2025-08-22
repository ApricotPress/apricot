using System;
using Apricot.Graphics.Textures;
using Apricot.Graphics.Vertices;
using Silk.NET.OpenGL;

namespace Apricot.OpenGl;

public static class GlConvertors
{
    public static void GetGlFormats(
        TextureFormat format,
        out InternalFormat internalFormat,
        out PixelFormat pixelFormat,
        out PixelType pixelType
    )
    {
        switch (format)
        {
            case TextureFormat.R8G8B8A8:
                internalFormat = InternalFormat.Rgba8;
                pixelFormat = PixelFormat.Rgba;
                pixelType = PixelType.UnsignedByte;
                return;


            case TextureFormat.R8:
                internalFormat = InternalFormat.R8;
                pixelFormat = PixelFormat.Red;
                pixelType = PixelType.UnsignedByte;
                return;

            case TextureFormat.R8G8:
                internalFormat = InternalFormat.RG8;
                pixelFormat = PixelFormat.RG;
                pixelType = PixelType.UnsignedByte;
                return;

            case TextureFormat.Depth24Stencil8:
                internalFormat = InternalFormat.Depth24Stencil8;
                pixelFormat = PixelFormat.DepthStencil;
                pixelType = PixelType.UnsignedInt248;
                return;

            case TextureFormat.Depth32Stencil8:
                internalFormat = InternalFormat.Depth32fStencil8;
                pixelFormat = PixelFormat.DepthStencil;
                pixelType = PixelType.Float32UnsignedInt248Rev;
                return;

            case TextureFormat.Depth16:
                internalFormat = InternalFormat.DepthComponent16;
                pixelFormat = PixelFormat.DepthComponent;
                pixelType = PixelType.UnsignedShort;
                return;

            case TextureFormat.Depth24:
                internalFormat = InternalFormat.DepthComponent24;
                pixelFormat = PixelFormat.DepthComponent;
                pixelType = PixelType.UnsignedInt;
                return;

            case TextureFormat.Depth32:
                internalFormat = InternalFormat.DepthComponent32f;
                pixelFormat = PixelFormat.DepthComponent;
                pixelType = PixelType.Float;
                return;

            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }
    }

    public static VertexAttribPointerType ToGl(this VertexElementFormat format) => 
        format switch
    {
        VertexElementFormat.Byte4 => VertexAttribPointerType.Byte,
        VertexElementFormat.Single => VertexAttribPointerType.Float,
        VertexElementFormat.Vector2 => VertexAttribPointerType.Float,
        VertexElementFormat.Vector3 => VertexAttribPointerType.Float,
        VertexElementFormat.Vector4 => VertexAttribPointerType.Float,
        VertexElementFormat.UByte4 => VertexAttribPointerType.UnsignedByte,
        VertexElementFormat.Short2 => VertexAttribPointerType.Short,
        VertexElementFormat.UShort2 => VertexAttribPointerType.UnsignedShort,
        VertexElementFormat.Short4 => VertexAttribPointerType.Short,
        VertexElementFormat.UShort4 => VertexAttribPointerType.UnsignedShort,
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
    };
}
