using System.Diagnostics;
using System.Runtime.CompilerServices;
using Apricot.Graphics;
using Apricot.Graphics.Buffers;
using Apricot.Graphics.Textures;
using Apricot.Sdl.Windows;
using Apricot.Windows;
using Microsoft.Extensions.Logging;
using SDL3;

namespace Apricot.Sdl.Graphics;

/// <summary>
/// Implemnentation of <see cref="IGraphics"/> layer with SDL_gpu. 
/// </summary>
public unsafe class SdlGpuGraphics(ILogger<SdlGpuGraphics> logger) : IGraphics
{
    private IntPtr _renderCommandBuffer;
    private IntPtr _currentRenderPass;
    private bool _fakeRenderPass;
    private IRenderTarget? _currentRenderTarget;


    private IntPtr _uploadCommandBuffer;
    private IntPtr _currentCopyPass;

    private readonly Dictionary<IWindow, SdlGpuWindowTarget> _windowTargets = new();
    private readonly Dictionary<IWindow, GpuSwapchainTexture> _swapchains = new();

    private readonly HashSet<Texture> _loadedTextures = [];

    public IntPtr GpuDeviceHandle { get; private set; }

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

        PrepareCommandBuffers();
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

        _loadedTextures.Add(texture);

        return texture;
    }

    public void SetTextureData(Texture texture, in ReadOnlySpan<byte> data)
    {
        if (texture.IsDisposed) throw new InvalidOperationException("Texture is disposed");

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
        Debug.Assert(texture.IsDisposed);

        SDL.SDL_ReleaseGPUTexture(GpuDeviceHandle, texture.Handle);
        _loadedTextures.Remove(texture);
    }

    public IndexBuffer CreateIndexBuffer(string? name, IndexSize indexSize)
    {
        throw new NotImplementedException();
    }
    public void Release(IndexBuffer buffer)
    {
        throw new NotImplementedException();
    }
    public IndexBuffer CreateVertexBuffer(string? name)
    {
        throw new NotImplementedException();
    }
    public void Release(VertexBuffer buffer)
    {
        throw new NotImplementedException();
    }

    public void SetRenderTarget(IRenderTarget target, Color? clearColor)
    {
        if (_currentRenderPass != IntPtr.Zero && _currentRenderTarget == target && !clearColor.HasValue) return;

        EndRenderPass();
        BeginRenderPass(target, clearColor);
    }

    public void Clear(Color color)
    {
        if (_currentRenderTarget is null)
        {
            throw new InvalidOperationException($"First call {nameof(SetRenderTarget)}");
        }

        var target = _currentRenderTarget;

        EndRenderPass();
        BeginRenderPass(target, color);
    }

    public void Present()
    {
        EndCopyPass();
        EndRenderPass();

        if (!SDL.SDL_SubmitGPUCommandBuffer(_uploadCommandBuffer))
        {
            throw SdlException.GetFromLatest(nameof(SDL.SDL_SubmitGPUCommandBuffer));
        }

        if (!SDL.SDL_SubmitGPUCommandBuffer(_renderCommandBuffer))
        {
            throw SdlException.GetFromLatest(nameof(SDL.SDL_SubmitGPUCommandBuffer));
        }

        _swapchains.Clear();

        _renderCommandBuffer = IntPtr.Zero;
        _uploadCommandBuffer = IntPtr.Zero;

        PrepareCommandBuffers();

        // todo: cleanup windows dictionary if any of window was closed
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (GpuDeviceHandle != IntPtr.Zero)
        {
            SDL.SDL_DestroyGPUDevice(GpuDeviceHandle);
        }
    }

    private void PrepareCommandBuffers()
    {
        if (_renderCommandBuffer != IntPtr.Zero || _uploadCommandBuffer != IntPtr.Zero)
        {
            throw new InvalidOperationException("Previous command buffer was not dispatched");
        }

        _renderCommandBuffer = SDL.SDL_AcquireGPUCommandBuffer(GpuDeviceHandle);
        _uploadCommandBuffer = SDL.SDL_AcquireGPUCommandBuffer(GpuDeviceHandle);
    }

    private void BeginRenderPass(IRenderTarget target, Color? clearColor)
    {
        logger.LogTrace(
            "Beginning render pass with target of {RenderTarget} and clear color {ClearColor}",
            target,
            clearColor
        );

        var targetInfo = target switch
        {
            SdlGpuWindowTarget { Window: { } window } => ColorTargetInfoFromWindow(window, clearColor),
            _ => throw new NotSupportedException($"Not supported render target: {target}")
        };

        if (targetInfo is null) // swapchain wasnt acquired, we should skip pass
        {
            _currentRenderTarget = target;
            _currentRenderPass = IntPtr.Zero;
            _fakeRenderPass = true;

            logger.LogTrace("Render pass was started as a fake one as target was not acquired");
        }
        else
        {
            Span<SDL.SDL_GPUColorTargetInfo> targetInfoSpan = stackalloc SDL.SDL_GPUColorTargetInfo[1];
            targetInfoSpan[0] = targetInfo.Value;

            scoped ref var depthTarget = ref Unsafe.NullRef<SDL.SDL_GPUDepthStencilTargetInfo>();

            _currentRenderPass = SDL.SDL_BeginGPURenderPass(
                _renderCommandBuffer,
                targetInfoSpan,
                1,
                depthTarget
            );
            _currentRenderTarget = target;
            _fakeRenderPass = false;

            logger.LogTrace("Successfully started render pass {RenderPass}", _currentRenderTarget);
        }
    }

    private void EndRenderPass()
    {
        if (_currentRenderPass != IntPtr.Zero)
        {
            logger.LogTrace("Ending render pass of {RenderPass}", _currentRenderTarget);
            SDL.SDL_EndGPURenderPass(_currentRenderPass);
        }

        _currentRenderPass = IntPtr.Zero;
        _currentRenderTarget = null;
        _fakeRenderPass = false;
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

    private SDL.SDL_GPUColorTargetInfo? ColorTargetInfoFromWindow(SdlWindow window, Color? clearColor)
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
}
