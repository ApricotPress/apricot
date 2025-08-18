using System.Reflection;
using Apricot.Assets;
using Apricot.Common;

namespace Apricot;

public static class BuiltInAssets
{
    public static class Shaders
    {
        public const string StandardVertex = "BuiltIn/Shaders/Standard.vert";
        public const string StandardFragment = "BuiltIn/Shaders/Standard.frag";
    }

    public static void Add(PreBakedAssetsImporter preBakedAssetsImporter)
    {
        AddShader(preBakedAssetsImporter, Shaders.StandardVertex);
        AddShader(preBakedAssetsImporter, Shaders.StandardFragment);
    }

    private static void AddShader(PreBakedAssetsImporter preBaked, string name) =>
        preBaked.AddPreBaked(
            name,
            [
                new Artifact(
                    Guid.Empty,
                    name + " (dxil)",
                    new ArtifactTarget(null, GraphicDriver.Direct3d12),
                    ReadEmbeded(name + ".dxil")
                ),
                new Artifact(
                    Guid.Empty,
                    Shaders.StandardVertex + " (glsl)",
                    new ArtifactTarget(null, GraphicDriver.OpenGl),
                    ReadEmbeded(name + ".glsl")
                ),
                new Artifact(
                    Guid.Empty,
                    Shaders.StandardVertex + " (metal)",
                    new ArtifactTarget(null, GraphicDriver.Metal),
                    ReadEmbeded(name + ".msl")
                ),
                new Artifact(
                    Guid.Empty,
                    Shaders.StandardVertex + " (spir-v)",
                    new ArtifactTarget(null, GraphicDriver.Vulkan),
                    ReadEmbeded(name + ".spv")
                )
            ]
        );

    private static byte[] ReadEmbeded(string name)
    {
        using var stream = Assembly.GetCallingAssembly().GetManifestResourceStream(name);

        if (stream == null) throw new Exception($"Missing built-in asset file `{name}`");

        var result = new byte[stream.Length];
        stream.ReadExactly(result);
        return result;
    }
}
