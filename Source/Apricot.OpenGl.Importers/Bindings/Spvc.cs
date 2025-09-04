using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Apricot.OpenGl.Importers.Bindings;

public enum SpvcResult
{
    Success = 0,
    ErrorInvalidSpirv = -1,
    ErrorUnsupportedSpirv = -2,
    ErrorOutOfMemory = -3,
    ErrorInvalidArgument = -4,
}

public enum SpvcBackend
{
    None = 0,
    Glsl = 1,
    Hlsl = 2,
    Msl = 3,
    Cpp = 4,
    Json = 5
}

public enum SpvcCaptureMode
{
    Copy = 0,
    TakeOwnership = 1
}

public enum CompilerOptionBits
{
    Common = 0x1000000,
    Glsl = 0x2000000,
    Hlsl = 0x4000000,
    Msl = 0x8000000,
    Lang = 0x0f00000,
    Enum = 0xffffff
}

public enum CompilerOption
{
    Unknown = 0,

    ForceTemporary = 1 | CompilerOptionBits.Common,
    FlattenMultidimensionalArrays = 2 | CompilerOptionBits.Common,
    FixupDepthConvention = 3 | CompilerOptionBits.Common,
    FlipVertexY = 4 | CompilerOptionBits.Common,
    EmitLineDirectives = 37 | CompilerOptionBits.Common,
    EnableStorageImageQualifierDeduction = 52 | CompilerOptionBits.Common,
    ForceZeroInitializedVariables = 54 | CompilerOptionBits.Common,

    GlslSupportNonZeroBaseInstance = 5 | CompilerOptionBits.Glsl,
    GlslSeparateShaderObjects = 6 | CompilerOptionBits.Glsl,
    GlslEnable420PackExtension = 7 | CompilerOptionBits.Glsl,
    GlslVersion = 8 | CompilerOptionBits.Glsl,
    GlslEs = 9 | CompilerOptionBits.Glsl,
    GlslVulkanSemantics = 10 | CompilerOptionBits.Glsl,
    GlslEsDefaultFloatPrecisionHighp = 11 | CompilerOptionBits.Glsl,
    GlslEsDefaultIntPrecisionHighp = 12 | CompilerOptionBits.Glsl,
    GlslEmitPushConstantAsUniformBuffer = 33 | CompilerOptionBits.Glsl,
    GlslEmitUniformBufferAsPlainUniforms = 35 | CompilerOptionBits.Glsl,
    GlslForceFlattenedIoBlocks = 66 | CompilerOptionBits.Glsl,
    
    HlslShaderModel = 13 | CompilerOptionBits.Hlsl,
    HlslPointSizeCompat = 14 | CompilerOptionBits.Hlsl,
    HlslPointCoordCompat = 15 | CompilerOptionBits.Hlsl,
    HlslSupportNonZeroBaseVertexBaseInstance = 16 | CompilerOptionBits.Hlsl,
    HlslFlattenMatrixVertexInputSemantics = 71 | CompilerOptionBits.Hlsl,
    HlslForceStorageBufferAsUav = 53 | CompilerOptionBits.Hlsl,
    HlslNonWritableUavTextureAsSrv = 55 | CompilerOptionBits.Hlsl,
    
    MslVersion = 17 | CompilerOptionBits.Msl,
    MslTexelBufferTextureWidth = 18 | CompilerOptionBits.Msl,
    MslSwizzleBufferIndex = 19 | CompilerOptionBits.Msl,
    MslIndirectParamsBufferIndex = 20 | CompilerOptionBits.Msl,
    MslShaderOutputBufferIndex = 21 | CompilerOptionBits.Msl,
    MslShaderPatchOutputBufferIndex = 22 | CompilerOptionBits.Msl,
    MslShaderTessFactorOutputBufferIndex = 23 | CompilerOptionBits.Msl,
    MslShaderInputWorkgroupIndex = 24 | CompilerOptionBits.Msl,
    MslEnablePointSizeBuiltin = 25 | CompilerOptionBits.Msl,
    MslDisableRasterization = 26 | CompilerOptionBits.Msl,
    MslCaptureOutputToBuffer = 27 | CompilerOptionBits.Msl,
    MslSwizzleTextureSamples = 28 | CompilerOptionBits.Msl,
    MslPadFragmentOutputComponents = 29 | CompilerOptionBits.Msl,
    MslTessDomainOriginLowerLeft = 30 | CompilerOptionBits.Msl,
    MslPlatform = 31 | CompilerOptionBits.Msl,
    MslArgumentBuffers = 32 | CompilerOptionBits.Msl,
    MslTextureBufferNative = 34 | CompilerOptionBits.Msl,
    MslBufferSizeBufferIndex = 36 | CompilerOptionBits.Msl,
    MslMultiview = 38 | CompilerOptionBits.Msl,
    MslViewMaskBufferIndex = 39 | CompilerOptionBits.Msl,
    MslDeviceIndex = 40 | CompilerOptionBits.Msl,
    MslViewIndexFromDeviceIndex = 41 | CompilerOptionBits.Msl,
    MslDispatchBase = 42 | CompilerOptionBits.Msl,
    MslDynamicOffsetsBufferIndex = 43 | CompilerOptionBits.Msl,
    MslTexture1DAs2D = 44 | CompilerOptionBits.Msl,
    MslEnableBaseIndexZero = 45 | CompilerOptionBits.Msl,
    MslFramebufferFetchSubpass = 46 | CompilerOptionBits.Msl,
    MslInvariantFpMath = 47 | CompilerOptionBits.Msl,
    MslEmulateCubemapArray = 48 | CompilerOptionBits.Msl,
    MslEnableDecorationBinding = 49 | CompilerOptionBits.Msl,
    MslForceActiveArgumentBufferResources = 50 | CompilerOptionBits.Msl,
    MslForceNativeArrays = 51 | CompilerOptionBits.Msl,
    MslEnableFragOutputMask = 56 | CompilerOptionBits.Msl,
    MslEnableFragDepthBuiltin = 57 | CompilerOptionBits.Msl,
    MslEnableFragStencilRefBuiltin = 58 | CompilerOptionBits.Msl,
    MslEnableClipDistanceUserVarying = 59 | CompilerOptionBits.Msl,
    HlslEnable16BitTypes = 60 | CompilerOptionBits.Hlsl,
    MslMultiPatchWorkgroup = 61 | CompilerOptionBits.Msl,
    MslShaderInputBufferIndex = 62 | CompilerOptionBits.Msl,
    MslShaderIndexBufferIndex = 63 | CompilerOptionBits.Msl,
    MslVertexForTessellation = 64 | CompilerOptionBits.Msl,
    MslVertexIndexType = 65 | CompilerOptionBits.Msl,
    MslMultiviewLayeredRendering = 67 | CompilerOptionBits.Msl,
    MslArrayedSubpassInput = 68 | CompilerOptionBits.Msl,
    MslR32UiLinearTextureAlignment = 69 | CompilerOptionBits.Msl,
    MslR32UiAlignmentConstantId = 70 | CompilerOptionBits.Msl,
    MslIosUseSimdgroupFunctions = 72 | CompilerOptionBits.Msl,
    MslEmulateSubgroups = 73 | CompilerOptionBits.Msl,
    MslFixedSubgroupSize = 74 | CompilerOptionBits.Msl,
    MslForceSampleRateShading = 75 | CompilerOptionBits.Msl
}

public static partial class Spvc
{
    const string Lib = "spirv-cross-c-shared";

    [LibraryImport(Lib, EntryPoint = "spvc_context_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SpvcResult spvc_context_create(out IntPtr context);

    [LibraryImport(Lib, EntryPoint = "spvc_context_destroy")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void spvc_context_destroy(IntPtr context);

    [LibraryImport(Lib, EntryPoint = "spvc_context_get_last_error_string")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr spvc_context_get_last_error_string(IntPtr context);

    [LibraryImport(Lib, EntryPoint = "spvc_context_parse_spirv")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial SpvcResult spvc_context_parse_spirv(
        IntPtr context,
        uint* spirv,
        /* size in 32-bit words */ nuint word_count,
        out IntPtr parsed_ir
    );

    [LibraryImport(Lib, EntryPoint = "spvc_context_create_compiler")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SpvcResult spvc_context_create_compiler(
        IntPtr context,
        SpvcBackend backend,
        IntPtr parsed_ir,
        SpvcCaptureMode mode,
        out IntPtr compiler
    );

    [LibraryImport(Lib, EntryPoint = "spvc_compiler_create_compiler_options")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SpvcResult spvc_compiler_create_compiler_options(
        IntPtr compiler,
        out IntPtr options
    );

    [LibraryImport(Lib, EntryPoint = "spvc_compiler_options_set_bool")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SpvcResult spvc_compiler_options_set_bool(
        IntPtr options,
        CompilerOption option,
        byte value
    );

    [LibraryImport(Lib, EntryPoint = "spvc_compiler_options_set_uint")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SpvcResult spvc_compiler_options_set_uint(
        IntPtr options,
        CompilerOption option,
        uint value
    );

    [LibraryImport(Lib, EntryPoint = "spvc_compiler_install_compiler_options")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SpvcResult spvc_compiler_install_compiler_options(
        IntPtr compiler,
        IntPtr options
    );

    [LibraryImport(Lib, EntryPoint = "spvc_compiler_build_combined_image_samplers")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SpvcResult spvc_compiler_build_combined_image_samplers(IntPtr compiler);

    [LibraryImport(Lib, EntryPoint = "spvc_compiler_compile")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SpvcResult spvc_compiler_compile(
        IntPtr compiler,
        out IntPtr source /* const char* owned by context */
    );

    [LibraryImport(Lib, EntryPoint = "spvc_context_release_allocations")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void spvc_context_release_allocations(IntPtr context);

    public static void ThrowIfError(this SpvcResult res, IntPtr ctx)
    {
        if (res == SpvcResult.Success) return;

        var errPtr = spvc_context_get_last_error_string(ctx);
        var msg = errPtr != IntPtr.Zero
            ? Marshal.PtrToStringUTF8(errPtr) ?? "SPIRV-Cross error."
            : "SPIRV-Cross error.";

        throw new InvalidOperationException($"{res}: {msg}");
    }
}
