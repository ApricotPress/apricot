namespace Apricot.Graphics.Shaders;

public readonly record struct ShaderProgramDescription(
    byte[] Code,
    int SamplerCount,
    int UniformBufferCount,
    string EntryPoint = "main"
);
