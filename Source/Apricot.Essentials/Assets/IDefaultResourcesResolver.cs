using Apricot.Graphics.Materials;
using Apricot.Graphics.Shaders;

namespace Apricot.Essentials.Assets;

public interface IDefaultResourcesResolver
{
    ShaderProgram GetStandardShader(ShaderStage stage);

    Material GetStandardMaterial();
}
