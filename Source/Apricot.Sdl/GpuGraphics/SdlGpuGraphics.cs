using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Apricot.Graphics;
using Apricot.Graphics.Buffers;
using Apricot.Graphics.Commands;
using Apricot.Graphics.Materials;
using Apricot.Graphics.Shaders;
using Apricot.Graphics.Structs;
using Apricot.Graphics.Textures;
using Apricot.Graphics.Vertices;
using Apricot.Utils.Collections;
using Apricot.Sdl.Windows;
using Apricot.Windows;
using Microsoft.Extensions.Logging;
using SDL3;

namespace Apricot.Sdl.Graphics;

/// <summary>
/// Implemnentation of <see cref="IGraphics"/> layer with SDL_gpu. 
/// </summary>
public sealed unsafe class SdlGpuGraphics(ILogger<SdlGpuGraphics> logger) : IGraphics
{
    private IntPtr _renderCommandBuffer;
    private RenderPassState _renderPass;

    private GraphicDriver _driver = GraphicDriver.Unknown;

    private IntPtr _uploadCommandBuffer;
    private IntPtr _currentCopyPass;

    private readonly Dictionary<IWindow, SdlGpuWindowTarget> _windowTargets = new();
    private readonly Dictionary<IWindow, GpuSwapchainTexture> _swapchains = new();
    private readonly Dictionary<int, IntPtr> _renderPipelinesCache = new();
    private readonly Dictionary<TextureSampler, IntPtr> _samplersCache = new();

    private readonly HashSet<Texture> _loadedTextures = [];
    private readonly HashSet<GraphicBuffer> _loadedBuffers = [];
    private readonly HashSet<ShaderProgram> _loadedShaders = [];

    private Texture? _emptyTexture;

    public IntPtr GpuDeviceHandle { get; private set; }

    public Texture EmptyTexture => _emptyTexture ?? throw new InvalidOperationException("Device is not initialized");

    ~SdlGpuGraphics() => Dispose(false);

    public void Initialize(GraphicDriver preferredDriver, bool enableDebug)
    {
        if (GpuDeviceHandle != IntPtr.Zero)
        {
            throw new InvalidOperationException("GPU device is already initialized.");
        }

        var driverName = preferredDriver switch
        {
            GraphicDriver.Metal => "metal",
            GraphicDriver.Direct3d12 => "direct3d12",
            GraphicDriver.Vulkan => "vulkan",
            _ => null,
        };

        // todo: properly configure
        GpuDeviceHandle = SDL.SDL_CreateGPUDevice(
            SDL.SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV |
            SDL.SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_DXIL |
            SDL.SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_MSL,
            enableDebug,
            driverName!
        );

        if (GpuDeviceHandle == IntPtr.Zero)
        {
            SdlException.ThrowFromLatest(nameof(SDL.SDL_CreateGPUDevice));
        }

        var effectiveDriver = SDL.SDL_GetGPUDeviceDriver(GpuDeviceHandle);
        _driver = effectiveDriver switch
        {
            "metal" => GraphicDriver.Metal,
            "vulkan" => GraphicDriver.Vulkan,
            "direct3d12" => GraphicDriver.Direct3d12,
            _ => GraphicDriver.Unknown
        };

        PrepareCommandBuffers();

        _emptyTexture = CreateTexture("Fallback", 1, 1);
        SetTextureData(_emptyTexture, [255, 0, 255, 255]); // magenta
    }

    public void SetVsync(IWindow window, bool vsync)
    {
        if (!_windowTargets.TryGetValue(window, out var windowHandle))
        {
            throw new InvalidOperationException(
                $"Window {window} is not a valid render target and therefore graphics device can't control its VSync"
            );
        }

        var supportsMailbox = SDL.SDL_WindowSupportsGPUPresentMode(
            GpuDeviceHandle,
            windowHandle.Window.Handle,
            SDL.SDL_GPUPresentMode.SDL_GPU_PRESENTMODE_MAILBOX
        );

        SDL.SDL_SetGPUSwapchainParameters(
            GpuDeviceHandle,
            windowHandle.Window.Handle,
            swapchain_composition: SDL.SDL_GPUSwapchainComposition.SDL_GPU_SWAPCHAINCOMPOSITION_SDR,
            present_mode: supportsMailbox
                ? SDL.SDL_GPUPresentMode.SDL_GPU_PRESENTMODE_MAILBOX
                : SDL.SDL_GPUPresentMode.SDL_GPU_PRESENTMODE_IMMEDIATE
        );
    }

    public IRenderTarget GetWindowRenderTarget(IWindow window)
    {
        if (_windowTargets.TryGetValue(window, out var target))
        {
            return target;
        }

        if (window is not SdlWindow sdlWindow)
        {
            throw new NotSupportedException("Sdl graphics only supports SdlWindow");
        }

        _windowTargets[window] = new SdlGpuWindowTarget(this, sdlWindow);
        return _windowTargets[window];
    }

    public Texture CreateTexture(
        string? name,
        int width,
        int height,
        TextureFormat format = TextureFormat.R8G8B8A8,
        TextureUsage usage = TextureUsage.Sampling
    )
    {
        logger.LogTrace(
            "Creating texture of name {name} {width}x{height} with format {format} for {usage}",
            name,
            width,
            height,
            format,
            usage
        );

        uint props = 0;

        if (!string.IsNullOrEmpty(name))
        {
            props = SDL.SDL_CreateProperties();
            SDL.SDL_SetStringProperty(props, SDL.SDL_PROP_GPU_TEXTURE_CREATE_NAME_STRING, name);
        }

        SDL.SDL_GPUTextureCreateInfo info = new()
        {
            type = SDL.SDL_GPUTextureType.SDL_GPU_TEXTURETYPE_2D,
            format = format.ToSdl(),
            usage = usage.ToSdl(),
            width = (uint)width,
            height = (uint)height,
            layer_count_or_depth = 1,
            num_levels = 1,
            sample_count = SDL.SDL_GPUSampleCount.SDL_GPU_SAMPLECOUNT_1,
            props = props
        };

        var handle = SDL.SDL_CreateGPUTexture(GpuDeviceHandle, info);

        if (props != 0)
        {
            SDL.SDL_DestroyProperties(props);
        }

        if (handle == IntPtr.Zero)
        {
            SdlException.ThrowFromLatest(nameof(SDL.SDL_CreateGPUTexture));
        }

        Texture texture = new(this, name ?? handle.ToString(), width, height, handle, format);

        lock (_loadedTextures)
        {
            _loadedTextures.Add(texture);
        }

        return texture;
    }

    public void SetTextureData(Texture texture, in ReadOnlySpan<byte> data)
    {
        if (texture.IsDisposed) throw new InvalidOperationException($"{texture} is disposed");

        logger.LogTrace("Setting data of texture {texture}", texture);

        var transferBuffer = SDL.SDL_CreateGPUTransferBuffer(
            GpuDeviceHandle,
            new SDL.SDL_GPUTransferBufferCreateInfo
            {
                usage = SDL.SDL_GPUTransferBufferUsage.SDL_GPU_TRANSFERBUFFERUSAGE_UPLOAD,
                size = (uint)data.Length,
                props = 0
            }
        );

        var dstPointer = SDL.SDL_MapGPUTransferBuffer(GpuDeviceHandle, transferBuffer, true);
        var dstSpan = new Span<byte>(dstPointer.ToPointer(), data.Length);
        data.CopyTo(dstSpan);

        SDL.SDL_UnmapGPUTransferBuffer(GpuDeviceHandle, transferBuffer);

        BeginCopyPass();
        SDL.SDL_UploadToGPUTexture(
            _currentCopyPass,
            source: new SDL.SDL_GPUTextureTransferInfo
            {
                transfer_buffer = transferBuffer,
                offset = 0,
                pixels_per_row = (uint)texture.Width,
                rows_per_layer = (uint)texture.Height
            },
            destination: new SDL.SDL_GPUTextureRegion
            {
                texture = texture.Handle,
                layer = 0,
                mip_level = 0,
                x = 0,
                y = 0,
                z = 0,
                w = (uint)texture.Width,
                h = (uint)texture.Height,
                d = 1
            },
            cycle: false
        );

        SDL.SDL_ReleaseGPUTransferBuffer(GpuDeviceHandle, transferBuffer);
    }

    public void Release(Texture texture)
    {
        if (texture.IsDisposed) throw new InvalidOperationException($"{texture} is already disposed.");

        logger.LogTrace("Releasing texture {texture}", texture);

        SDL.SDL_ReleaseGPUTexture(GpuDeviceHandle, texture.Handle);
        lock (_loadedTextures) _loadedTextures.Remove(texture);
    }

    public IndexBuffer CreateIndexBuffer(string? name, IndexSize indexSize, int capacity)
    {
        logger.LogTrace(
            "Creating index buffer with name {name} with {capacity} elements of {size} size",
            name,
            capacity,
            indexSize
        );

        var nativeBuffer = CreateGraphicBuffer(
            name,
            (uint)((int)indexSize * capacity),
            SDL.SDL_GPUBufferUsageFlags.SDL_GPU_BUFFERUSAGE_INDEX
        );

        var buffer = new IndexBuffer(
            this,
            name ?? nativeBuffer.ToString(),
            capacity,
            indexSize,
            nativeBuffer
        );

        lock (_loadedBuffers)
        {
            _loadedBuffers.Add(buffer);
        }

        return buffer;
    }

    public void Release(IndexBuffer buffer)
    {
        if (buffer.IsDisposed) throw new InvalidOperationException($"{buffer.Name} is already disposed.");

        SDL.SDL_ReleaseGPUBuffer(GpuDeviceHandle, buffer.NativePointer);
    }

    public VertexBuffer CreateVertexBuffer(string? name, VertexFormat format, int capacity) =>
        CreateVertexBuffer<VertexBuffer>(
            name,
            format,
            capacity,
            (n, handle) => new VertexBuffer(this, n, format, capacity, handle)
        );

    public VertexBuffer<T> CreateVertexBuffer<T>(string? name, int capacity) where T : unmanaged, IVertex =>
        CreateVertexBuffer<VertexBuffer<T>>(
            name,
            T.Format,
            capacity,
            (n, handle) => new VertexBuffer<T>(this, n, capacity, handle)
        );

    public void Release(VertexBuffer buffer)
    {
        if (buffer.IsDisposed) throw new InvalidOperationException($"{buffer.Name} is already disposed.");

        SDL.SDL_ReleaseGPUBuffer(GpuDeviceHandle, buffer.NativePointer);
    }

    public void UploadBufferData<T>(GraphicBuffer buffer, in ReadOnlySpan<T> data) where T : unmanaged
    {
        if (buffer.IsDisposed) throw new InvalidOperationException($"{buffer.Name} is disposed.");

        logger.LogTrace("Uploading data {buffer}", buffer.Name);

        var dataSize = data.Length * Marshal.SizeOf<T>();
        if (dataSize > buffer.Capacity * buffer.ElementSize)
        {
            throw new InvalidOperationException("Buffer is too short");
        }

        var transferBuffer = SDL.SDL_CreateGPUTransferBuffer(
            GpuDeviceHandle,
            new SDL.SDL_GPUTransferBufferCreateInfo
            {
                usage = SDL.SDL_GPUTransferBufferUsage.SDL_GPU_TRANSFERBUFFERUSAGE_UPLOAD,
                size = (uint)dataSize
            }
        );

        var dstPointer = SDL.SDL_MapGPUTransferBuffer(GpuDeviceHandle, transferBuffer, false).ToPointer();
        var dstSpan = new Span<byte>(dstPointer, dataSize);
        var srcSpan = MemoryMarshal.AsBytes(data);
        srcSpan.CopyTo(dstSpan);
        SDL.SDL_UnmapGPUTransferBuffer(GpuDeviceHandle, transferBuffer);

        BeginCopyPass();
        SDL.SDL_UploadToGPUBuffer(
            _currentCopyPass,
            new SDL.SDL_GPUTransferBufferLocation
            {
                transfer_buffer = transferBuffer,
                offset = 0
            },
            new SDL.SDL_GPUBufferRegion
            {
                buffer = buffer.NativePointer,
                offset = 0,
                size = (uint)dataSize
            },
            false
        );

        SDL.SDL_ReleaseGPUTransferBuffer(GpuDeviceHandle, transferBuffer);
    }

    public ShaderProgram CreateShaderProgram(string? name, in ShaderProgramDescription description)
    {
        var format = _driver switch
        {
            GraphicDriver.Metal => SDL.SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_MSL,
            GraphicDriver.Vulkan => SDL.SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV,
            GraphicDriver.Direct3d12 => SDL.SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_DXIL,
            _ => SDL.SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV
        };

        var entryPoint = Encoding.UTF8.GetBytes(description.EntryPoint);
        IntPtr nativeShader;

        fixed (byte* entryPointPtr = entryPoint)
        fixed (byte* code = description.Code)
            nativeShader = SDL.SDL_CreateGPUShader(
                GpuDeviceHandle,
                new SDL.SDL_GPUShaderCreateInfo
                {
                    code_size = (nuint)description.Code.Length,
                    code = code,
                    entrypoint = entryPointPtr,
                    format = format,
                    stage = description.Stage == ShaderStage.Fragment
                        ? SDL.SDL_GPUShaderStage.SDL_GPU_SHADERSTAGE_FRAGMENT
                        : SDL.SDL_GPUShaderStage.SDL_GPU_SHADERSTAGE_VERTEX,
                    num_samplers = (uint)description.SamplerCount,
                    num_storage_textures = 0,
                    num_storage_buffers = 0,
                    num_uniform_buffers = (uint)description.UniformBufferCount,
                }
            );

        if (nativeShader == IntPtr.Zero) SdlException.ThrowFromLatest(nameof(SDL.SDL_CreateGPUShader));

        var shader = new ShaderProgram(
            this,
            name ?? nativeShader.ToString(),
            nativeShader,
            description
        );

        lock (_loadedShaders) _loadedShaders.Add(shader);

        return shader;
    }

    public void Release(ShaderProgram shaderProgram)
    {
        if (shaderProgram.IsDisposed)
            throw new InvalidOperationException($"Shader {shaderProgram} is already released.");

        SDL.SDL_ReleaseGPUShader(GpuDeviceHandle, shaderProgram.Handle);

        lock (_loadedShaders) _loadedShaders.Remove(shaderProgram);
    }

    public void SetRenderTarget(IRenderTarget target, Color? clearColor) =>
        _renderPass.SetRenderTarget(target, clearColor);

    public void Clear(Color color)
    {
        if (_renderPass.CurrentRenderTarget is null)
        {
            throw new InvalidOperationException($"First call {nameof(SetRenderTarget)}");
        }

        var target = _renderPass.CurrentRenderTarget;

        _renderPass.EndRenderPass();
        _renderPass.BeginRenderPass(target, color);
    }

    public void Submit(DrawCommand command)
    {
        // todo: add warnings
        if (command is { IndexBuffer: not null, IndicesCount: 0 }) return;

        _renderPass.SetRenderTarget(command.Target);
        _renderPass.SetViewport(command.Viewport ?? command.Target.Viewport);
        _renderPass.SetScissors(command.Scissors ?? command.Target.Viewport);

        _renderPass.SetPipeline(GetGraphicsPipeline(command));

        _renderPass.SetIndexBuffer(command.IndexBuffer);
        _renderPass.SetVertexBuffer(command.VertexBuffer);

        _renderPass.SetFragmentSamplers(command.Material.FragmentStage.Samplers);
        _renderPass.SetVertexSamplers(command.Material.VertexStage.Samplers);

        _renderPass.SetFragmentUniform(command.Material.FragmentStage.UniformBuffer);
        _renderPass.SetVertexUniform(command.Material.VertexStage.UniformBuffer);


        // perform draw
        if (command.IndexBuffer != null)
        {
            _renderPass.DrawIndexed(command.IndicesCount, command.IndicesOffset, command.VerticesOffset);
            
        }
        else
        {
            _renderPass.Draw(command.VerticesCount, command.VerticesOffset);
        }
    }

    public void Present()
    {
        SubmitCommands(false);

        _swapchains.Clear();

        // todo: cleanup windows dictionary if any of window was closed
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (GpuDeviceHandle != IntPtr.Zero)
        {
            SDL.SDL_DestroyGPUDevice(GpuDeviceHandle);
        }
    }

    private void SubmitCommands(bool wait)
    {
        EndCopyPass();
        _renderPass.EndRenderPass();

        var fences = new StackList4<IntPtr>();

        if (wait)
        {
            CollectFence(_uploadCommandBuffer);
            CollectFence(_renderCommandBuffer);
        }
        else
        {
            if (!SDL.SDL_SubmitGPUCommandBuffer(_uploadCommandBuffer) ||
                !SDL.SDL_SubmitGPUCommandBuffer(_renderCommandBuffer))
            {
                throw SdlException.GetFromLatest(nameof(SDL.SDL_SubmitGPUCommandBuffer));
            }
        }

        _renderCommandBuffer = _uploadCommandBuffer = 0;
        PrepareCommandBuffers();

        if (fences.Count > 0) SDL.SDL_WaitForGPUFences(GpuDeviceHandle, true, fences.Span, (uint)fences.Count);

        foreach (var fence in fences)
        {
            SDL.SDL_ReleaseGPUFence(GpuDeviceHandle, fence);
        }

        void CollectFence(IntPtr commandBuffer)
        {
            var fence = SDL.SDL_SubmitGPUCommandBufferAndAcquireFence(commandBuffer);

            if (fence == IntPtr.Zero)
            {
                logger.LogWarning("Failed to acquire fence: {Error}", SDL.SDL_GetError());
            }
            else
            {
                fences.Add(fence);
            }
        }
    }

    private void PrepareCommandBuffers()
    {
        if (_renderCommandBuffer != IntPtr.Zero || _uploadCommandBuffer != IntPtr.Zero)
        {
            throw new InvalidOperationException("Previous command buffer was not dispatched");
        }

        _uploadCommandBuffer = SDL.SDL_AcquireGPUCommandBuffer(GpuDeviceHandle);
        _renderCommandBuffer = SDL.SDL_AcquireGPUCommandBuffer(GpuDeviceHandle);

        _renderPass = new RenderPassState(this, _renderCommandBuffer, logger);
    }

    private void BeginCopyPass()
    {
        if (_currentCopyPass != IntPtr.Zero) return;

        _currentCopyPass = SDL.SDL_BeginGPUCopyPass(_uploadCommandBuffer);

        if (_currentCopyPass == IntPtr.Zero)
        {
            SdlException.ThrowFromLatest(nameof(SDL.SDL_BeginGPUCopyPass));
        }
    }

    private void EndCopyPass()
    {
        if (_currentCopyPass == IntPtr.Zero) return;

        SDL.SDL_EndGPUCopyPass(_currentCopyPass);
        _currentCopyPass = IntPtr.Zero;
    }

    public SDL.SDL_GPUColorTargetInfo? ColorTargetInfoFromWindow(SdlWindow window, Color? clearColor)
    {
        var texture = GetSwapchainForWindow(window, true);

        return texture.TextureHandle == IntPtr.Zero
            ? null
            : new SDL.SDL_GPUColorTargetInfo
            {
                texture = texture.TextureHandle,
                clear_color = clearColor.HasValue
                    ? new SDL.SDL_FColor
                    {
                        r = clearColor.Value.R,
                        g = clearColor.Value.G,
                        b = clearColor.Value.B,
                        a = clearColor.Value.A
                    }
                    : default,
                load_op = clearColor.HasValue
                    ? SDL.SDL_GPULoadOp.SDL_GPU_LOADOP_CLEAR
                    : SDL.SDL_GPULoadOp.SDL_GPU_LOADOP_DONT_CARE,
                store_op = SDL.SDL_GPUStoreOp.SDL_GPU_STOREOP_STORE
            };
    }

    private GpuSwapchainTexture GetSwapchainForWindow(SdlWindow window, bool wait)
    {
        if (_swapchains.TryGetValue(window, out var swapchain))
        {
            return swapchain;
        }

        if (wait)
        {
            if (!SDL.SDL_AcquireGPUSwapchainTexture(
                    _renderCommandBuffer,
                    window.Handle,
                    out var texture,
                    out var w,
                    out var h
                ))
            {
                SdlException.ThrowFromLatest(nameof(SDL.SDL_AcquireGPUSwapchainTexture));
            }

            return _swapchains[window] = new GpuSwapchainTexture(texture, w, h);
        }
        else
        {
            if (!SDL.SDL_WaitAndAcquireGPUSwapchainTexture(
                    _renderCommandBuffer,
                    window.Handle,
                    out var texture,
                    out var w,
                    out var h
                ))
            {
                SdlException.ThrowFromLatest(nameof(SDL.SDL_WaitAndAcquireGPUSwapchainTexture));
            }

            return _swapchains[window] = new GpuSwapchainTexture(texture, w, h);
        }
    }

    private T CreateVertexBuffer<T>(
        string? name,
        VertexFormat format,
        int capacity,
        Func<string, IntPtr, T> factory
    ) where T : VertexBuffer
    {
        logger.LogTrace(
            "Creating vertex buffer with name {name} with {capacity} elements of {vertex} vertex format",
            name,
            capacity,
            format
        );

        var nativeBuffer = CreateGraphicBuffer(
            name,
            (uint)(format.Stride * capacity),
            SDL.SDL_GPUBufferUsageFlags.SDL_GPU_BUFFERUSAGE_VERTEX
        );

        var buffer = factory(
            name ?? nativeBuffer.ToString(),
            nativeBuffer
        );

        lock (_loadedBuffers) _loadedBuffers.Add(buffer);

        return buffer;
    }

    private IntPtr CreateGraphicBuffer(string? name, uint size, SDL.SDL_GPUBufferUsageFlags usage)
    {
        uint props = 0;
        if (!string.IsNullOrEmpty(name))
        {
            props = SDL.SDL_CreateProperties();
            SDL.SDL_SetStringProperty(props, SDL.SDL_PROP_GPU_BUFFER_CREATE_NAME_STRING, name);
        }

        var buffer = SDL.SDL_CreateGPUBuffer(
            GpuDeviceHandle,
            new SDL.SDL_GPUBufferCreateInfo
            {
                usage = usage,
                size = size,
                props = props
            }
        );

        if (props != 0)
        {
            SDL.SDL_DestroyProperties(props);
        }

        if (buffer == IntPtr.Zero)
        {
            SdlException.ThrowFromLatest(nameof(SDL.SDL_CreateGPUBuffer));
        }

        return buffer;
    }

    private IntPtr GetGraphicsPipeline(DrawCommand command)
    {
        var hash = HashCode.Combine(
            command.Material.FragmentStage.ShaderProgram.Handle,
            command.Material.VertexStage.ShaderProgram.Handle,
            command.CullMode,
            command.DepthCompare,
            command.DepthTestEnabled,
            command.DepthWriteEnabled,
            command.BlendMode
        );

        hash = HashCode.Combine(
            hash,
            command.IndexBuffer?.IndexSize,
            command.VertexBuffer.Format
        );

        return _renderPipelinesCache.TryGetValue(hash, out var pipeline)
            ? pipeline
            : _renderPipelinesCache[hash] = BuildGraphicsPipeline(command);
    }

    private IntPtr BuildGraphicsPipeline(DrawCommand command)
    {
        var vertexBindings = stackalloc SDL.SDL_GPUVertexBufferDescription[1];
        var vertexAttributes = stackalloc SDL.SDL_GPUVertexAttribute[command.VertexBuffer.Format.Elements.Count];

        vertexBindings[0] = new SDL.SDL_GPUVertexBufferDescription
        {
            input_rate = SDL.SDL_GPUVertexInputRate.SDL_GPU_VERTEXINPUTRATE_VERTEX,
            instance_step_rate = 0,
            pitch = (uint)command.VertexBuffer.ElementSize,
            slot = 0
        };

        uint offset = 0;
        for (var i = 0; i < command.VertexBuffer.Format.Elements.Count; i++)
        {
            var vertexElement = command.VertexBuffer.Format.Elements[i];

            vertexAttributes[i] = new SDL.SDL_GPUVertexAttribute
            {
                buffer_slot = 0,
                format = vertexElement.Format.ToSdl(vertexElement.Normalized),
                location = (uint)vertexElement.Location,
                offset = offset
            };
            offset += (uint)vertexElement.Format.Size();
        }

        // todo: implement proper color attachments work
        var colorAttachments = stackalloc SDL.SDL_GPUColorTargetDescription[]
        {
            new SDL.SDL_GPUColorTargetDescription
            {
                format = SDL.SDL_GetGPUSwapchainTextureFormat(GpuDeviceHandle,
                    ((SdlGpuWindowTarget)command.Target).Window.Handle),
                blend_state = command.BlendMode.ToSdl()
            }
        };

        var pipeline = SDL.SDL_CreateGPUGraphicsPipeline(
            GpuDeviceHandle,
            new SDL.SDL_GPUGraphicsPipelineCreateInfo
            {
                vertex_shader = command.Material.VertexStage.ShaderProgram.Handle,
                fragment_shader = command.Material.FragmentStage.ShaderProgram.Handle,
                vertex_input_state = new SDL.SDL_GPUVertexInputState
                {
                    vertex_buffer_descriptions = vertexBindings,
                    num_vertex_buffers = 1,
                    vertex_attributes = vertexAttributes,
                    num_vertex_attributes = (uint)command.VertexBuffer.Format.Elements.Count
                },
                primitive_type = SDL.SDL_GPUPrimitiveType.SDL_GPU_PRIMITIVETYPE_TRIANGLELIST,
                rasterizer_state = new SDL.SDL_GPURasterizerState
                {
                    fill_mode = SDL.SDL_GPUFillMode.SDL_GPU_FILLMODE_FILL, // todo: add different fill modes
                    cull_mode = command.CullMode.ToSdl(),
                    front_face = SDL.SDL_GPUFrontFace.SDL_GPU_FRONTFACE_CLOCKWISE,
                    enable_depth_bias = false
                },
                multisample_state = new SDL.SDL_GPUMultisampleState
                {
                    sample_count = SDL.SDL_GPUSampleCount.SDL_GPU_SAMPLECOUNT_1,
                    sample_mask = 0
                },
                depth_stencil_state = new SDL.SDL_GPUDepthStencilState
                {
                    compare_op = command.DepthCompare.ToSdl(),
                    back_stencil_state = default,
                    front_stencil_state = default,
                    compare_mask = 0xff,
                    write_mask = 0xff,
                    enable_depth_test = command.DepthTestEnabled,
                    enable_depth_write = command.DepthWriteEnabled,
                    enable_stencil_test = false
                },
                target_info = new SDL.SDL_GPUGraphicsPipelineTargetInfo
                {
                    color_target_descriptions = colorAttachments,
                    num_color_targets = 1,
                    has_depth_stencil_target = false,
                    depth_stencil_format = SDL.SDL_GPUTextureFormat.SDL_GPU_TEXTUREFORMAT_INVALID
                }
            }
        );

        if (pipeline == IntPtr.Zero)
        {
            SdlException.ThrowFromLatest(nameof(SDL.SDL_CreateGPUGraphicsPipeline));
        }

        return pipeline;
    }

    public nint GetSampler(in TextureSampler sampler)
    {
        if (_samplersCache.TryGetValue(sampler, out var result)) return result;

        SDL.SDL_GPUSamplerCreateInfo info = new()
        {
            min_filter = sampler.Filter.ToSdl(),
            mag_filter = sampler.Filter.ToSdl(),
            address_mode_u = sampler.WrapU.ToSdl(),
            address_mode_v = sampler.WrapV.ToSdl(),
            address_mode_w = sampler.WrapW.ToSdl(),
            compare_op = SDL.SDL_GPUCompareOp.SDL_GPU_COMPAREOP_ALWAYS,
            enable_compare = false
        };

        result = SDL.SDL_CreateGPUSampler(GpuDeviceHandle, info);
        if (result == IntPtr.Zero)
        {
            SdlException.ThrowFromLatest(nameof(SDL.SDL_CreateGPUSampler));
        }

        return _samplersCache[sampler] = result;
    }
}
