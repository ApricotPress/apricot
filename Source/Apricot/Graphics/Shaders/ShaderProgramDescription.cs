using MessagePack;

namespace Apricot.Graphics.Shaders;

[MessagePackObject(true)]
public readonly record struct ShaderProgramDescription(
    byte[] Code,
    int SamplerCount,
    int UniformBufferCount,
    ShaderStage Stage,
    string EntryPoint
);
