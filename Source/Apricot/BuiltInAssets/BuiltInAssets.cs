using System.Reflection;
using Apricot.Assets;
using Apricot.Common;

namespace Apricot;

/// <summary>
/// Class for adding built-in embedded assets to <see cref="PreBakedAssetsImporter"/> and for accessing their ids.
/// </summary>
public static class BuiltInAssets
{
    /// <summary>
    /// List of constants with ids that should be used for <see cref="IAssetsDatabase"/> as paths.
    /// </summary>
    public static class Shaders
    {
        public const string StandardVertex = "BuiltIn/Shaders/Standard.vert";

        public const string StandardFragment = "BuiltIn/Shaders/Standard.frag";
    }

    /// <summary>
    /// Adds built-in assets to pre-baked importer.
    /// </summary>
    /// <param name="preBakedAssetsImporter">Pre-baked importer.</param>
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
                    name + " (dxil)",
                    new ArtifactTarget(null, GraphicDriver.Direct3d12),
                    ReadEmbeded(name + ".dxil")
                ),
                new Artifact(
                    Shaders.StandardVertex + " (glsl)",
                    new ArtifactTarget(null, GraphicDriver.OpenGl),
                    ReadEmbeded(name + ".glsl")
                ),
                new Artifact(
                    Shaders.StandardVertex + " (metal)",
                    new ArtifactTarget(null, GraphicDriver.Metal),
                    ReadEmbeded(name + ".msl")
                ),
                new Artifact(
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
