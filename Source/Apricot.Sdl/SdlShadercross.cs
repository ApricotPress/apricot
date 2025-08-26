using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace

namespace SDL3.ShaderCross;

public static unsafe partial class SdlShaderCross
{
    private const string nativeLibName = "SDL3_shadercross";

    public const int SDL_SHADERCROSS_MAJOR_VERSION = 3;
    public const int SDL_SHADERCROSS_MINOR_VERSION = 0;
    public const int SDL_SHADERCROSS_MICRO_VERSION = 0;

    public const string SDL_SHADERCROSS_PROP_SPIRV_PSSL_COMPATIBILITY = "SDL.shadercross.spirv.pssl.compatibility";
    public const string SDL_SHADERCROSS_PROP_SPIRV_MSL_VERSION = "SDL.shadercross.spirv.msl.version";

    // -------- Enums --------
    public enum SDL_ShaderCross_IOVarType
    {
        SDL_SHADERCROSS_IOVAR_TYPE_UNKNOWN,
        SDL_SHADERCROSS_IOVAR_TYPE_INT8,
        SDL_SHADERCROSS_IOVAR_TYPE_UINT8,
        SDL_SHADERCROSS_IOVAR_TYPE_INT16,
        SDL_SHADERCROSS_IOVAR_TYPE_UINT16,
        SDL_SHADERCROSS_IOVAR_TYPE_INT32,
        SDL_SHADERCROSS_IOVAR_TYPE_UINT32,
        SDL_SHADERCROSS_IOVAR_TYPE_INT64,
        SDL_SHADERCROSS_IOVAR_TYPE_UINT64,
        SDL_SHADERCROSS_IOVAR_TYPE_FLOAT16,
        SDL_SHADERCROSS_IOVAR_TYPE_FLOAT32,
        SDL_SHADERCROSS_IOVAR_TYPE_FLOAT64
    }

    public enum SDL_ShaderCross_ShaderStage
    {
        SDL_SHADERCROSS_SHADERSTAGE_VERTEX,
        SDL_SHADERCROSS_SHADERSTAGE_FRAGMENT,
        SDL_SHADERCROSS_SHADERSTAGE_COMPUTE
    }

    // -------- Structs --------
    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_ShaderCross_IOVarMetadata
    {
        public byte* name; // UTF-8 char*
        public uint location;
        public SDL_ShaderCross_IOVarType vector_type;
        public uint vector_size;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_ShaderCross_GraphicsShaderMetadata
    {
        public uint num_samplers;
        public uint num_storage_textures;
        public uint num_storage_buffers;
        public uint num_uniform_buffers;
        public uint num_inputs;
        public SDL_ShaderCross_IOVarMetadata* inputs; // array length = num_inputs
        public uint num_outputs;
        public SDL_ShaderCross_IOVarMetadata* outputs; // array length = num_outputs
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_ShaderCross_ComputePipelineMetadata
    {
        public uint num_samplers;
        public uint num_readonly_storage_textures;
        public uint num_readonly_storage_buffers;
        public uint num_readwrite_storage_textures;
        public uint num_readwrite_storage_buffers;
        public uint num_uniform_buffers;
        public uint threadcount_x;
        public uint threadcount_y;
        public uint threadcount_z;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_ShaderCross_SPIRV_Info
    {
        public byte* bytecode; // const Uint8*
        public nuint bytecode_size; // size_t
        public byte* entrypoint; // const char* (UTF-8)
        public SDL_ShaderCross_ShaderStage shader_stage;
        public SDL.SDLBool enable_debug; // bool
        public byte* name; // const char* (UTF-8) or NULL
        public uint props; // SDL_PropertiesID (uint)
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_ShaderCross_HLSL_Define
    {
        public byte* name; // char*
        public byte* value; // char* or NULL
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_ShaderCross_HLSL_Info
    {
        public byte* source; // const char*
        public byte* entrypoint; // const char*
        public byte* include_dir; // const char* or NULL
        public SDL_ShaderCross_HLSL_Define* defines; // array terminated by a fully NULL struct, or NULL
        public SDL_ShaderCross_ShaderStage shader_stage;
        public SDL.SDLBool enable_debug;
        public byte* name; // const char* or NULL
        public uint props;
    }
    
    [LibraryImport(nativeLibName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDL.SDLBool SDL_ShaderCross_Init();

    [LibraryImport(nativeLibName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void SDL_ShaderCross_Quit();

    [LibraryImport(nativeLibName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDL.SDL_GPUShaderFormat SDL_ShaderCross_GetSPIRVShaderFormats();

    [LibraryImport(nativeLibName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDL.CallerOwnedStringMarshaller))]
    public static partial string SDL_ShaderCross_TranspileMSLFromSPIRV(
        in SDL_ShaderCross_SPIRV_Info info
    ); 

    [LibraryImport(nativeLibName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDL.CallerOwnedStringMarshaller))]
    public static partial string SDL_ShaderCross_TranspileHLSLFromSPIRV(
        in SDL_ShaderCross_SPIRV_Info info
    ); 

    [LibraryImport(nativeLibName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr SDL_ShaderCross_CompileDXBCFromSPIRV(
        in SDL_ShaderCross_SPIRV_Info info,
        out nuint size
    );

    [LibraryImport(nativeLibName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr SDL_ShaderCross_CompileDXILFromSPIRV(
        in SDL_ShaderCross_SPIRV_Info info,
        out nuint size
    );

    [LibraryImport(nativeLibName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr SDL_ShaderCross_CompileGraphicsShaderFromSPIRV(
        IntPtr device,
        ref SDL_ShaderCross_SPIRV_Info info,
        in SDL_ShaderCross_GraphicsShaderMetadata metadata,
        uint props
    );

    [LibraryImport(nativeLibName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr SDL_ShaderCross_CompileComputePipelineFromSPIRV(
        IntPtr device,
        in SDL_ShaderCross_SPIRV_Info info,
        in SDL_ShaderCross_ComputePipelineMetadata metadata,
        uint props
    );

    [LibraryImport(nativeLibName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDL_ShaderCross_GraphicsShaderMetadata* SDL_ShaderCross_ReflectGraphicsSPIRV(
        byte* bytecode,
        nuint bytecode_size,
        uint props
    ); 

    [LibraryImport(nativeLibName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDL_ShaderCross_ComputePipelineMetadata* SDL_ShaderCross_ReflectComputeSPIRV(
        byte* bytecode,
        nuint bytecode_size,
        uint props
    );

    [LibraryImport(nativeLibName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDL.SDL_GPUShaderFormat SDL_ShaderCross_GetHLSLShaderFormats();

    [LibraryImport(nativeLibName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr SDL_ShaderCross_CompileDXBCFromHLSL(
        in SDL_ShaderCross_HLSL_Info info,
        out nuint size
    );

    [LibraryImport(nativeLibName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr SDL_ShaderCross_CompileDXILFromHLSL(
        in SDL_ShaderCross_HLSL_Info info,
        out nuint size
    );

    [LibraryImport(nativeLibName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr SDL_ShaderCross_CompileSPIRVFromHLSL(
        in SDL_ShaderCross_HLSL_Info info,
        out nuint size
    );
}
