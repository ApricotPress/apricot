using System.Runtime.InteropServices;
using Apricot.Graphics.Shaders;

namespace Apricot.Graphics.Materials;

public class Stage(ShaderProgram shaderProgram)
{
    public ShaderProgram ShaderProgram = shaderProgram;

    public BoundSampler[] Samplers = new BoundSampler[16];

    public byte[] UniformBuffer = [];

    public unsafe void SetUniformBuffer<T>(in T data) where T : unmanaged
    {
        fixed (T* ptr = &data)
            SetUniformBuffer(new ReadOnlySpan<byte>((byte*)ptr, Marshal.SizeOf<T>()));
    }

    public void SetUniformBuffer(in ReadOnlySpan<byte> data)
    {
        if (data.Length > UniformBuffer.Length)
            Array.Resize(ref UniformBuffer, data.Length);
        data.CopyTo(UniformBuffer);
    }
}
