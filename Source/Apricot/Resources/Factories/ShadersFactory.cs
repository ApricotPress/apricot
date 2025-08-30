using Apricot.Assets;
using Apricot.Assets.Artifacts;
using Apricot.Graphics;
using Apricot.Graphics.Shaders;
using Apricot.Platform;

namespace Apricot.Resources.Factories;

/// <summary>
/// Loads <see cref="ShaderProgramDescription"/> from artifact associated with shader program and creates
/// <see cref="ShaderProgram"/> from it using active <see cref="IGraphics"/>. 
/// </summary>
public class ShadersFactory(
    IAssetDatabase assets,
    IArtifactsDatabase artifacts,
    IPlatformInfo platform,
    IGraphics graphics
) : AssetBasedFactory<ShaderProgram>(assets, artifacts, platform)
{
    protected override ShaderProgram Construct(Asset asset, Artifact artifact)
    {
        if (artifact.Data is not ShaderProgramDescription programDescription)
        {
            throw new InvalidOperationException(
                $"Wrong artifact underlying type. Expected ShaderProgramDescription but got {artifact.Data.GetType()}"
            );
        }

        return graphics.CreateShaderProgram(asset.Name, programDescription);
    }
}
