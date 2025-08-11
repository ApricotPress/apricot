using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Apricot.Graphics;
using Apricot.Graphics.Buffers;
using Apricot.Graphics.Materials;
using Apricot.Graphics.Structs;
using Apricot.Utils.Collections;
using Microsoft.Extensions.Logging;
using SDL3;

namespace Apricot.Sdl.Graphics;

public struct RenderPassState(SdlGpuGraphics graphics, IntPtr commandBuffer, ILogger logger)
{
    private IntPtr _handle;

    private IRenderTarget? _currentRenderTarget;
    private IntPtr _currentPipeline;
    private Rect _currentViewport;
    private Rect _currentScissors;
    private VertexBuffer? _currentVertexBuffer;
    private IndexBuffer? _currentIndexBuffer;
    private StackList16<SDL.SDL_GPUTextureSamplerBinding> _fragmentSamplers = [];
    private StackList16<SDL.SDL_GPUTextureSamplerBinding> _vertexSamplers = [];
    private bool _fakeRenderPass;


    public void Clear(Color color)
    {
        if (_currentRenderTarget is null)
        {
            throw new InvalidOperationException($"First set render target");
        }

        var target = _currentRenderTarget;
        EndRenderPass();
        BeginRenderPass(target, color);
    }

    [MemberNotNull(nameof(_currentRenderTarget))]
    public void BeginRenderPass(IRenderTarget target, Color? clearColor = null)
    {
        logger.LogTrace(
            "Beginning render pass with target of {RenderTarget} and clear color {ClearColor}",
            target,
            clearColor
        );

        var targetInfo = target switch
        {
            SdlGpuWindowTarget { Window: { } window } => graphics.ColorTargetInfoFromWindow(window, clearColor),
            _ => throw new NotSupportedException($"Not supported render target: {target}")
        };

        if (targetInfo is null) // swapchain wasnt acquired, we should skip pass
        {
            _currentRenderTarget = target;
            _handle = IntPtr.Zero;
            _fakeRenderPass = true;

            logger.LogTrace("Render pass was started as a fake one as target was not acquired");
        }
        else
        {
            Span<SDL.SDL_GPUColorTargetInfo> targetInfoSpan = stackalloc SDL.SDL_GPUColorTargetInfo[1];
            targetInfoSpan[0] = targetInfo.Value;

            scoped ref var depthTarget = ref Unsafe.NullRef<SDL.SDL_GPUDepthStencilTargetInfo>();

            _handle = SDL.SDL_BeginGPURenderPass(
                commandBuffer,
                targetInfoSpan,
                1,
                depthTarget
            );
            _currentRenderTarget = target;
            _fakeRenderPass = false;

            logger.LogTrace("Successfully started render pass {RenderPass}", _currentRenderTarget);
        }
    }

    public void EndRenderPass()
    {
        if (_handle != IntPtr.Zero)
        {
            logger.LogTrace("Ending render pass of {RenderPass}", _currentRenderTarget);
            SDL.SDL_EndGPURenderPass(_handle);
        }

        _handle = IntPtr.Zero;
        _currentRenderTarget = null;
        _currentPipeline = IntPtr.Zero;
        _currentVertexBuffer = null;
        _currentIndexBuffer = null;
        _fakeRenderPass = false;
    }

    [MemberNotNull(nameof(_currentRenderTarget))]
    public void SetRenderTarget(IRenderTarget target, Color? clearColor = null)
    {
        if (_handle != IntPtr.Zero && _currentRenderTarget == target && !clearColor.HasValue) return;

        EndRenderPass();
        BeginRenderPass(target, clearColor);
    }

    public void SetViewport(Rect viewport)
    {
        if (_currentViewport == viewport) return;

        _currentViewport = viewport;

        SDL.SDL_SetGPUViewport(
            _handle,
            new SDL.SDL_GPUViewport()
            {
                x = viewport.X,
                y = viewport.Y,
                w = viewport.Width,
                h = viewport.Height,
                min_depth = 0,
                max_depth = 1
            }
        );
    }

    public void SetScissors(Rect scissors)
    {
        if (_currentScissors == scissors) return;

        _currentScissors = scissors;
        SDL.SDL_SetGPUScissor(
            _handle,
            new SDL.SDL_Rect
            {
                x = scissors.X,
                y = scissors.Y,
                w = scissors.Width,
                h = scissors.Height
            }
        );
    }

    public void SetPipeline(IntPtr pipeline)
    {
        if (_currentPipeline == pipeline) return;

        _currentPipeline = pipeline;
        SDL.SDL_BindGPUGraphicsPipeline(_handle, pipeline);
    }

    public void SetIndexBuffer(IndexBuffer? indexBuffer)
    {
        if (_currentIndexBuffer == indexBuffer) return;

        _currentIndexBuffer = indexBuffer;
        if (_currentIndexBuffer != null)
        {
            SDL.SDL_BindGPUIndexBuffer(
                _handle,
                new SDL.SDL_GPUBufferBinding
                {
                    buffer = _currentIndexBuffer.NativePointer,
                    offset = 0
                },
                _currentIndexBuffer.IndexSize.ToSdl()
            );
        }
    }

    public void SetVertexBuffer(VertexBuffer vertexBuffer)
    {
        _currentVertexBuffer = vertexBuffer;
        if (_currentVertexBuffer != null)
        {
            SDL.SDL_BindGPUVertexBuffers(
                _handle,
                0,
                [
                    new SDL.SDL_GPUBufferBinding
                    {
                        buffer = _currentVertexBuffer.NativePointer,
                        offset = 0
                    }
                ],
                1
            );
        }
    }

    public void SetFragmentSamplers(BoundSampler[] samplers)
    {
        var sdlSamplers = BuildSamplers(samplers);

        if (!SamplersEquals(sdlSamplers, _fragmentSamplers))
        {
            _fragmentSamplers = sdlSamplers;
            SDL.SDL_BindGPUFragmentSamplers(
                _handle,
                0,
                sdlSamplers.Span,
                (uint)samplers.Length
            );
        }
    }

    public void SetVertexSamplers(BoundSampler[] samplers)
    {
        var sdlSamplers = BuildSamplers(samplers);

        if (SamplersEquals(sdlSamplers, _vertexSamplers)) return;

        _vertexSamplers = sdlSamplers;
        SDL.SDL_BindGPUVertexSamplers(
            _handle,
            0,
            sdlSamplers.Span,
            (uint)samplers.Length
        );
    }

    public unsafe void SetFragmentUniform(byte[] uniform)
    {
        fixed (byte* ptr = uniform)
            SDL.SDL_PushGPUFragmentUniformData(
                commandBuffer,
                0,
                new IntPtr(ptr),
                (uint)uniform.Length
            );
    }

    public unsafe void SetVertexUniform(byte[] uniform)
    {
        fixed (byte* ptr = uniform)
            SDL.SDL_PushGPUVertexUniformData(
                commandBuffer,
                0,
                new IntPtr(ptr),
                (uint)uniform.Length
            );
    }

    public void DrawIndexed(int indicesCount, int indicesOffset, int verticesOffset) =>
        SDL.SDL_DrawGPUIndexedPrimitives(
            render_pass: _handle,
            num_indices: (uint)indicesCount,
            num_instances: 1,
            first_index: (uint)indicesOffset,
            vertex_offset: verticesOffset,
            first_instance: 0
        );

    public void Draw(int verticesCount, int commandVerticesOffset) =>
        SDL.SDL_DrawGPUPrimitives(
            render_pass: _handle,
            num_vertices: (uint)verticesCount,
            num_instances: 1,
            first_vertex: (uint)verticesCount,
            first_instance: 0
        );

    private static bool SamplersEquals(
        StackList16<SDL.SDL_GPUTextureSamplerBinding> a,
        StackList16<SDL.SDL_GPUTextureSamplerBinding> b
    )
    {
        if (a.Count != b.Count) return false;

        for (var i = 0; i < a.Count; i++)
        {
            if (a[i].sampler != b[i].sampler) return false;
            if (a[i].texture != b[i].texture) return false;
        }

        return true;
    }

    private StackList16<SDL.SDL_GPUTextureSamplerBinding> BuildSamplers(BoundSampler[] samplers)
    {
        var newSamplers = new StackList16<SDL.SDL_GPUTextureSamplerBinding>();
        foreach (var sampler in samplers)
        {
            newSamplers.Add(new SDL.SDL_GPUTextureSamplerBinding()
            {
                texture = sampler.Texture is { IsDisposed: false } tex
                    ? tex.Handle
                    : graphics.EmptyTexture.Handle,
                sampler = graphics.GetSampler(sampler.Sampler)
            });
        }

        return newSamplers;
    }
}
