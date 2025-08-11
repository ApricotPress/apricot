using Apricot.Graphics.Textures;

namespace Apricot.Graphics.Materials;

public readonly record struct BoundSampler(Texture? Texture, TextureSampler Sampler);

public readonly record struct TextureSampler(
    FilterMode Filter,
    WrapMode WrapU,
    WrapMode WrapV,
    WrapMode WrapW
)
{
    public TextureSampler(FilterMode filter, WrapMode wrap) : this(filter, wrap, wrap, wrap) { }
}
