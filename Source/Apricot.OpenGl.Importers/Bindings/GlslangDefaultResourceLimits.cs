using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Apricot.OpenGl.Importers.Bindings;

public static partial class GlslangDefaultResourceLimits
{
    private const string LibName = "glslang-default-resource-limits";

    [LibraryImport(LibName, EntryPoint = "glslang_default_resource")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr DefaultResource();
}
