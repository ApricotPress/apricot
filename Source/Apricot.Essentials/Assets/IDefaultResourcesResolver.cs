using Apricot.Graphics.Materials;
using Apricot.Graphics.Shaders;
using Apricot.Graphics.Textures;

namespace Apricot.Essentials.Assets;

public interface IDefaultResourcesResolver
{
    ShaderProgram GetStandardShader(ShaderStage stage);

    Material GetStandardMaterial();
    
    Texture GetEmptyTexture();
}
