using System.Runtime.CompilerServices;
using Apricot.Graphics;
using Apricot.Sdl.Windows;
using Apricot.Windows;
using Microsoft.Extensions.Logging;
using SDL3;

namespace Apricot.Sdl.Graphics;

public unsafe class SdlGraphics(ILogger<SdlGraphics> logger) : IGraphics, IDisposable
{
    private string? _gpuDriver;

    private IntPtr _renderCommandBuffer;
    private IntPtr _currentRenderPass;
    private IRenderTarget? _currentRenderTarget;

    public IntPtr GpuDeviceHandle { get; private set; }

    ~SdlGraphics() => Dispose(false);

    public void Initialize()
    {
        if (GpuDeviceHandle != IntPtr.Zero)
        {
            throw new InvalidOperationException("GPU device is already initialized.");
        }

        _gpuDriver = SDL.SDL_GetGPUDriver(0);

        // todo: properly configure
        GpuDeviceHandle = SDL.SDL_CreateGPUDevice(
            SDL.SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV |
            SDL.SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_DXIL |
            SDL.SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_MSL,
            true,
            _gpuDriver
        );

        if (GpuDeviceHandle == IntPtr.Zero)
        {
            SdlException.ThrowFromLatest(nameof(SDL.SDL_CreateGPUDevice));
        }

        PrepareCommandBuffers();
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

    public IRenderTarget GetWindowRenderTarget(IWindow window)
    {
        if (window is not SdlWindow sdlWindow)
        {
            throw new NotSupportedException("Sdl graphics only supports SdlWindow");
        }

        return new SdlWindowTarget(this, sdlWindow);
    }

    public void SetRenderTarget(IRenderTarget target, Color? clearColor)
    {
        if (_currentRenderPass != IntPtr.Zero && _currentRenderTarget == target && !clearColor.HasValue) return;

        EndRenderPass();
        BeginRenderPass(target, clearColor);
    }

    public void Present()
    {
        EndRenderPass();

        if (!SDL.SDL_SubmitGPUCommandBuffer(_renderCommandBuffer))
        {
            throw SdlException.GetFromLatest(nameof(SDL.SDL_SubmitGPUCommandBuffer));
        }

        _renderCommandBuffer = IntPtr.Zero;
        PrepareCommandBuffers();
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
        if (_renderCommandBuffer != IntPtr.Zero)
        {
            throw new InvalidOperationException("Previous command buffer was not dispatched");
        }

        _renderCommandBuffer = SDL.SDL_AcquireGPUCommandBuffer(GpuDeviceHandle);
    }

    private void BeginRenderPass(IRenderTarget target, Color? clearColor)
    {
        Span<SDL.SDL_GPUColorTargetInfo> targetInfoSpan = stackalloc SDL.SDL_GPUColorTargetInfo[1];
        targetInfoSpan[0] = target switch
        {
            SdlWindowTarget { Window.Handle: var window } => ColorTargetInfoFromWindow(window, clearColor),
            _ => throw new NotSupportedException($"Not supported render target: {target}")
        };
        scoped ref var depthTarget = ref Unsafe.NullRef<SDL.SDL_GPUDepthStencilTargetInfo>();


        _currentRenderPass = SDL.SDL_BeginGPURenderPass(
            _renderCommandBuffer,
            targetInfoSpan,
            1,
            depthTarget
        );
        _currentRenderTarget = target;
    }

    private SDL.SDL_GPUColorTargetInfo ColorTargetInfoFromWindow(IntPtr window, Color? clearColor)
    {
        if (!SDL.SDL_WaitAndAcquireGPUSwapchainTexture(_renderCommandBuffer, window, out var texture, out _, out _))
        {
            SdlException.ThrowFromLatest(nameof(SDL.SDL_WaitAndAcquireGPUSwapchainTexture));
        }

        if (texture == IntPtr.Zero)
        {
            logger.LogError("Cry");
            return default;
        }

        return new SDL.SDL_GPUColorTargetInfo
        {
            texture = texture,
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

    private void EndRenderPass()
    {
        if (_currentRenderPass != IntPtr.Zero)
        {
            SDL.SDL_EndGPURenderPass(_currentRenderPass);
        }

        _currentRenderPass = IntPtr.Zero;
        _currentRenderTarget = null;
    }
}
