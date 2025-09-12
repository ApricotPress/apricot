using Apricot.Graphics;
using Apricot.Graphics.Materials;
using Apricot.Graphics.Shaders;
using Apricot.Platform;
using Apricot.Resources;

namespace Apricot.Essentials.Assets;

public class DefaultResourcesResolver(IPlatformInfo platform, IResourcesLoader resources) : IDefaultResourcesResolver
{
    public ShaderProgram GetStandardShader(ShaderStage stage)
    {
        var assetId = (stage, platform.GraphicDriver) switch
        {
            (ShaderStage.Fragment, GraphicDriver.OpenGl) => EssentialsIds.Shaders.StandardGlslFragment,
            (ShaderStage.Vertex, GraphicDriver.OpenGl) => EssentialsIds.Shaders.StandardGlslVertex,
            (ShaderStage.Fragment, _) => EssentialsIds.Shaders.StandardHlslFragment,
            (ShaderStage.Vertex, _) => EssentialsIds.Shaders.StandardHlslVertex,
            _ => throw new ArgumentOutOfRangeException()
        };

        return resources.Load<ShaderProgram>(assetId);
    }

    public Material GetStandardMaterial() => new(
        GetStandardShader(ShaderStage.Vertex),
        GetStandardShader(ShaderStage.Fragment)
    );
}
