using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Apricot.Sdl.Importers.Bindings;

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
