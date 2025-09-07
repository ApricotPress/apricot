using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Apricot.OpenGl.Importers.Bindings;

public partial class Glslang
{
    public const string LibName = "glslang";

    [LibraryImport(LibName, EntryPoint = "glslang_shader_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr ShaderCreate(in Input input);


    [LibraryImport(LibName, EntryPoint = "glslang_shader_set_entry_point", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void ShaderSetEntryPoint(IntPtr shader, string entryPoint);

    [LibraryImport(LibName, EntryPoint = "glslang_shader_preprocess")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int ShaderPreprocess(IntPtr shader, in Input input);

    [LibraryImport(LibName, EntryPoint = "glslang_shader_parse")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial GlslangBool ShaderParse(IntPtr shader, in Input input);

    [LibraryImport(LibName, EntryPoint = "glslang_shader_get_info_log", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(GlslOwnedStringMarshaller))]
    public static unsafe partial string ShaderGetInfoLog(IntPtr shader);

    [LibraryImport(LibName, EntryPoint = "glslang_shader_get_info_debug_log",
        StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(GlslOwnedStringMarshaller))]
    public static unsafe partial string ShaderGetInfoDebugLog(IntPtr shader);

    [LibraryImport(LibName, EntryPoint = "glslang_shader_get_preprocessed_code",
        StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial string ShaderGetPreprocessedCode(IntPtr shader);

    [LibraryImport(LibName, EntryPoint = "glslang_shader_delete")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void ShaderDelete(IntPtr shader);


    [LibraryImport(LibName, EntryPoint = "glslang_program_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial IntPtr ProgramCreate();

    [LibraryImport(LibName, EntryPoint = "glslang_program_add_shader")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial void ProgramAddShader(IntPtr program, IntPtr shader);

    [LibraryImport(LibName, EntryPoint = "glslang_program_link")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial GlslangBool ProgramLink(IntPtr program, Messages messages);

    [LibraryImport(LibName, EntryPoint = "glslang_program_SPIRV_generate")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial void ProgramSpirvGenerate(IntPtr program, Stage stage);

    [LibraryImport(LibName, EntryPoint = "glslang_program_SPIRV_generate_with_options")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial void ProgramSpirvGenerate(IntPtr program, Stage stage, SpvOptions options);

    [LibraryImport(LibName, EntryPoint = "glslang_program_SPIRV_get_size")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial nuint ProgramSpirvGetSize(IntPtr program);

    [LibraryImport(LibName, EntryPoint = "glslang_program_SPIRV_get")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial void ProgramSpirvGet(IntPtr program, uint* words);

    [LibraryImport(LibName, EntryPoint = "glslang_program_SPIRV_get_messages",
        StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial string ProgramSpirvGetMessages(IntPtr program);


    [LibraryImport(LibName, EntryPoint = "glslang_program_get_info_log", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(GlslOwnedStringMarshaller))]
    public static unsafe partial string ProgramGetInfoLog(IntPtr program);

    [LibraryImport(LibName, EntryPoint = "glslang_program_get_info_debug_log",
        StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(GlslOwnedStringMarshaller))]
    public static unsafe partial string ProgramGetInfoDebugLog(IntPtr program);

    [LibraryImport(LibName, EntryPoint = "glslang_program_delete")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial void ProgramDelete(IntPtr program);


    [CustomMarshaller(typeof(string), MarshalMode.ManagedToUnmanagedOut, typeof(GlslOwnedStringMarshaller))]
    public static unsafe class GlslOwnedStringMarshaller
    {
        public static string? ConvertToManaged(byte* unmanaged) => Marshal.PtrToStringUTF8((IntPtr)unmanaged);
    }

    public readonly record struct GlslangBool
    {
        private readonly int _value;

        private const int FalseValue = 0;
        private const int TrueValue = 1;

        internal GlslangBool(int value) => _value = value;

        public static implicit operator bool(GlslangBool b) => b._value != FalseValue;

        public static implicit operator GlslangBool(bool b) => new(b ? TrueValue : FalseValue);

        public bool Equals(GlslangBool other) => other._value == _value;

        public override int GetHashCode() => _value.GetHashCode();
    }

    public enum SourceType
    {
        None = 0,
        Glsl = 1,
        Hlsl = 2
    }

    public enum Stage
    {
        Vertex,
        TessControl,
        TessEvaluation,
        Geometry,
        Fragment,
        Compute,
        Raygen,
        Intersect,
        AnyHit,
        ClosestHit,
        Miss,
        Callable,
        Task,
        Mesh
    }

    public enum Client
    {
        None,
        Vulkan,
        OpenGl
    }

    [Flags]
    public enum TargetClientVersion
    {
        Vulkan10 = (1 << 22),
        Vulkan11 = (1 << 22) | (1 << 12),
        Vulkan12 = (1 << 22) | (2 << 12),
        Vulkan13 = (1 << 22) | (3 << 12),
        Vulkan14 = (1 << 22) | (4 << 12),
        Opengl450 = 450,
    }

    public enum TargetLanguage
    {
        None,
        Spirv
    }

    [Flags]
    public enum TargetLanguageVersion
    {
        Spirv10 = (1 << 16),
        Spirv11 = (1 << 16) | (1 << 8),
        Spirv12 = (1 << 16) | (2 << 8),
        Spirv13 = (1 << 16) | (3 << 8),
        Spirv14 = (1 << 16) | (4 << 8),
        Spirv15 = (1 << 16) | (5 << 8),
        Spirv16 = (1 << 16) | (6 << 8),
    }

    [Flags]
    public enum Profile
    {
        Bad = 0,
        No = 1 << 0,
        Core = 1 << 1,
        Compatibility = 1 << 2,
        Es = 1 << 3
    }

    [Flags]
    public enum Messages
    {
        DefaultBit = 0,
        RelaxedErrorsBit = 1 << 0,
        SuppressWarningsBit = 1 << 1,
        AstBit = 1 << 2,
        SpvRulesBit = 1 << 3,
        VulkanRulesBit = 1 << 4,
        OnlyPreprocessorBit = 1 << 5,
        ReadHlslBit = 1 << 6,
        CascadingErrorsBit = 1 << 7,
        KeepUncalledBit = 1 << 8,
        HlslOffsetsBit = 1 << 9,
        DebugInfoBit = 1 << 10,
        HlslEnable16BitTypesBit = 1 << 11,
        HlslLegalizationBit = 1 << 12,
        HlslDx9CompatibleBit = 1 << 13,
        BuiltinSymbolTableBit = 1 << 14,
        Enhanced = 1 << 15,
        AbsolutePath = 1 << 16,
        DisplayErrorColumn = 1 << 17,
        LinkTimeOptimizationBit = 1 << 18,
        ValidateCrossStageIoBit = 1 << 19,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SpvOptions
    {
        public int generateDebugInfo;
        public int stripDebugInfo;
        public int disableOptimizer;
        public int optimizeSize;
        public int disassemble;
        public int validate;
        public int emitNonsemanticShaderDebugInfo;
        public int emitNonsemanticShaderDebugSource;
        public int compileOnly;
        public int optimizeAllowExpandedIdBound;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IncludeCallbacks
    {
        [MarshalAs(UnmanagedType.FunctionPtr)] public IntPtr include_system;
        [MarshalAs(UnmanagedType.FunctionPtr)] public IntPtr include_local;
        [MarshalAs(UnmanagedType.FunctionPtr)] public IntPtr free_include_result;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Input
    {
        public SourceType language;
        public Stage stage;
        public Client client;
        public TargetClientVersion client_version;
        public TargetLanguage target_language;
        public TargetLanguageVersion target_language_version;
        public char* code;
        public int default_version;
        public Profile default_profile;
        public int force_default_version_and_profile;
        public int forward_compatible;
        public Messages messages;
        public IntPtr resource;
        public IncludeCallbacks callbacks;
        public IntPtr callbacks_ctx;
    }
}
