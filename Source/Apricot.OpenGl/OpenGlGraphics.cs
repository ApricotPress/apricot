using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Apricot.Graphics;
using Apricot.Graphics.Buffers;
using Apricot.Graphics.Commands;
using Apricot.Graphics.Shaders;
using Apricot.Graphics.Structs;
using Apricot.Graphics.Textures;
using Apricot.Graphics.Vertices;
using Apricot.Windows;
using Microsoft.Extensions.Logging;
using Silk.NET.OpenGL;
using Texture = Apricot.Graphics.Textures.Texture;

namespace Apricot.OpenGl;

public unsafe sealed class OpenGlGraphics(
    IGlPlatform glPlatform,
    IWindowsManager windows,
    ILogger<OpenGlGraphics> logger
) : IGraphics
{
    private readonly Dictionary<IWindow, GlWindowTarget> _windowTargets = new();
    private readonly Dictionary<long, uint> _shadersCache = new();
    private readonly Dictionary<GL, uint> _tempVaos = new(); // used by MacOS

    private GlWindowTarget? _currentWindow;
    private Texture? _emptyTexture;

    public Texture EmptyTexture => _emptyTexture ?? throw new InvalidOperationException("Graphics is not initialized");

    public GraphicDriver Driver => GraphicDriver.OpenGl;

    public void Initialize(GraphicDriver preferredDriver, bool enableDebug)
    {
        var mainWindow = windows.GetOrCreateDefaultWindow();

        GetWindowRenderTarget(mainWindow);
        _currentWindow = _windowTargets[mainWindow];

        _emptyTexture = CreateTexture("Empty Texture", 1, 1);
        _emptyTexture.SetData([255, 0, 255, 0]);
    }

    private void CheckErrors(GL gl, string context)
    {
        GLEnum error;

        if ((error = gl.GetError()) != GLEnum.NoError)
        {
            throw new Exception($"GL Exception ({context}: {error}");
        }
    }

    public void SetVsync(IWindow window, bool vsync) => glPlatform.SwapInterval = vsync ? 1 : 0;

    public IRenderTarget GetWindowRenderTarget(IWindow window)
    {
        if (_windowTargets.TryGetValue(window, out var target))
        {
            return target;
        }

        _windowTargets[window] = new GlWindowTarget(window, glPlatform);

        var gl = _windowTargets[window].Gl;

        CheckErrors(gl, nameof(GetWindowRenderTarget));

        _tempVaos[gl] = gl.GenVertexArray();
        CheckErrors(gl, nameof(gl.GenVertexArray));
        gl.BindVertexArray(_tempVaos[gl]);
        CheckErrors(gl, nameof(gl.BindVertexArray));
        gl.BindVertexArray(0);
        CheckErrors(gl, nameof(gl.BindVertexArray));

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
        CheckCurrentWindow();

        var gl = _currentWindow.Gl;

        GlConvertors.GetGlFormats(format, out var internalFormat, out var pixelFormat, out var pixelType);

        var handle = gl.GenTexture();

        gl.BindTexture(TextureTarget.Texture2D, handle);

        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);

        scoped ref var pixelsRef = ref Unsafe.NullRef<byte>();

        gl.TexImage2D(
            TextureTarget.Texture2D,
            level: 0,
            internalformat: (int)internalFormat,
            width: (uint)width,
            height: (uint)height,
            border: 0,
            format: pixelFormat,
            type: pixelType,
            pixels: pixelsRef
        );

        gl.BindTexture(TextureTarget.Texture2D, 0);

        CheckErrors(gl, nameof(CreateTexture));

        return new Texture(
            this,
            name ?? handle.ToString(),
            width,
            height,
            new IntPtr(handle),
            format
        );
    }


    public void SetTextureData(Texture texture, in ReadOnlySpan<byte> data)
    {
        if (texture.IsDisposed) throw new InvalidOperationException($"Texture is disposed");

        CheckCurrentWindow();
        var gl = _currentWindow.Gl;
        var handle = (uint)texture.Handle;

        GlConvertors.GetGlFormats(texture.Format, out _, out var pixelFormat, out var pixelType);

        gl.BindTexture(TextureTarget.Texture2D, handle);
        fixed (byte* dataPtr = &data.GetPinnableReference())
            gl.TexSubImage2D(
                TextureTarget.Texture2D,
                0,
                0,
                0,
                (uint)texture.Width,
                (uint)texture.Height,
                pixelFormat,
                pixelType,
                dataPtr
            );
        gl.BindTexture(TextureTarget.Texture2D, 0);

        CheckErrors(gl, nameof(SetTextureData));
    }

    public void Release(Texture texture)
    {
        if (texture.IsDisposed) throw new InvalidOperationException($"Texture is disposed");


        CheckCurrentWindow();
        var gl = _currentWindow.Gl;
        var handle = (uint)texture.Handle;

        gl.DeleteTexture(handle);

        CheckErrors(gl, nameof(Release));
    }

    public IndexBuffer CreateIndexBuffer(string? name, IndexSize indexSize, int capacity)
    {
        CheckCurrentWindow();
        var gl = _currentWindow.Gl;

        var handle = gl.GenBuffer();

        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, handle);
        gl.BufferData(
            BufferTargetARB.ElementArrayBuffer,
            (nuint)((int)indexSize * capacity),
            null,
            BufferUsageARB.StaticDraw
        );

        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);

        CheckErrors(gl, nameof(CreateIndexBuffer));

        return new IndexBuffer(
            this,
            name ?? handle.ToString(),
            capacity,
            indexSize,
            new IntPtr(handle)
        );
    }

    public void Release(IndexBuffer buffer)
    {
        if (buffer.IsDisposed) throw new InvalidOperationException($"{buffer.Name} is already disposed.");
        CheckCurrentWindow();
        var gl = _currentWindow.Gl;

        gl.DeleteBuffer((uint)buffer.NativePointer);

        CheckErrors(gl, nameof(Release));
    }

    public VertexBuffer CreateVertexBuffer(string? name, VertexFormat format, int capacity)
    {
        CheckCurrentWindow();
        var gl = _currentWindow.Gl;

        var handle = gl.GenBuffer();

        gl.BindBuffer(BufferTargetARB.ArrayBuffer, handle);
        gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(format.Stride * capacity), null, BufferUsageARB.StaticDraw);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);

        CheckErrors(gl, nameof(CreateVertexBuffer));

        return new VertexBuffer(
            this,
            name ?? handle.ToString(),
            format,
            capacity,
            new IntPtr(handle)
        );
    }

    public VertexBuffer<T> CreateVertexBuffer<T>(string? name, int capacity) where T : unmanaged, IVertex
    {
        CheckCurrentWindow();
        var gl = _currentWindow.Gl;

        var handle = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, handle);
        CheckErrors(gl, nameof(gl.BindBuffer));

        var byteSize = (nuint)(T.Format.Stride * capacity);

        gl.BufferData(BufferTargetARB.ArrayBuffer, byteSize, null, BufferUsageARB.StaticDraw);
        CheckErrors(gl, nameof(gl.BufferData));

        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        CheckErrors(gl, nameof(gl.BindBuffer));

        return new VertexBuffer<T>(
            this,
            name ?? handle.ToString(),
            capacity,
            new IntPtr(handle)
        );
    }

    public void Release(VertexBuffer buffer)
    {
        if (buffer.IsDisposed) throw new InvalidOperationException($"{buffer.Name} is already disposed.");

        CheckCurrentWindow();
        var gl = _currentWindow.Gl;

        gl.DeleteBuffer((uint)buffer.NativePointer);
        CheckErrors(gl, nameof(Release));
    }

    public void UploadBufferData<T>(GraphicBuffer buffer, in ReadOnlySpan<T> data) where T : unmanaged
    {
        if (buffer.IsDisposed) throw new InvalidOperationException($"{buffer.Name} is disposed.");

        CheckCurrentWindow();
        var gl = _currentWindow.Gl;

        var dataSize = data.Length * Unsafe.SizeOf<T>();
        var capacityBytes = buffer.Capacity * buffer.ElementSize;
        if (dataSize > capacityBytes)
        {
            throw new InvalidOperationException("Buffer is too short");
        }

        var target = GetTargetFor(buffer);
        var handle = (uint)buffer.NativePointer;

        gl.BindBuffer(target, handle);
        CheckErrors(gl, nameof(gl.BindBuffer));

        fixed (T* src = data)
        {
            gl.BufferSubData(target, 0, (nuint)dataSize, src);
            CheckErrors(gl, nameof(gl.BufferSubData));
        }

        gl.BindBuffer(target, 0);
        CheckErrors(gl, nameof(gl.BindBuffer));
    }

    public ShaderProgram CreateShaderProgram(string? name, in ShaderProgramDescription description)
    {
        CheckCurrentWindow();
        var gl = _currentWindow.Gl;

        var source = Encoding.UTF8.GetString(description.Code);

        var shaderType = description.Stage == ShaderStage.Fragment
            ? ShaderType.FragmentShader
            : ShaderType.VertexShader;

        var shader = gl.CreateShader(shaderType);
        gl.ShaderSource(shader, source);
        gl.CompileShader(shader);

        gl.GetShader(shader, ShaderParameterName.CompileStatus, out var success);

        if (success == 0)
        {
            var log = gl.GetShaderInfoLog(shader);
            gl.DeleteShader(shader);
            throw new InvalidOperationException($"Failed to compile {description.Stage} shader: {log}");
        }


        return new ShaderProgram(
            this,
            name ?? shader.ToString(),
            new IntPtr(shader),
            description
        );
    }

    public void Release(ShaderProgram shaderProgram)
    {
        if (shaderProgram.IsDisposed) throw new InvalidOperationException($"Shader is already released.");

        CheckCurrentWindow();
        var gl = _currentWindow.Gl;

        gl.DeleteShader((uint)shaderProgram.Handle);

        CheckErrors(gl, nameof(Release));
    }

    [MemberNotNull(nameof(_currentWindow))]
    public void SetRenderTarget(IRenderTarget target, Color? clearColor)
    {
        if (target is GlWindowTarget window)
        {
            _currentWindow = window;
        }
        else
        {
            throw new NotSupportedException("Currently on GlWindowTarget is supported as a renderer");
        }

        if (clearColor.HasValue)
        {
            Clear(clearColor.Value);
        }
    }

    public void Clear(Color color)
    {
        CheckCurrentWindow();

        _currentWindow.Gl.ClearColor(
            color.R,
            color.G,
            color.B,
            color.A
        );
        _currentWindow.Gl.Clear(ClearBufferMask.ColorBufferBit);
        CheckErrors(_currentWindow.Gl, nameof(Clear));
    }

    public void Submit(DrawCommand command)
    {
        SetRenderTarget(command.Target, null);

        var gl = _currentWindow.Gl;

        var viewport = command.Viewport ?? command.Target.Viewport;
        gl.Viewport(viewport.X, viewport.Y, (uint)viewport.Width, (uint)viewport.Height);

        var scissors = command.Scissors ?? viewport;
        gl.Scissor(scissors.X, scissors.Y, (uint)scissors.Width, (uint)scissors.Height);

        gl.UseProgram(GetOrCreateProgram(
            command.Material.VertexStage.ShaderProgram,
            command.Material.FragmentStage.ShaderProgram
        ));

        // todo: blend

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            gl.BindVertexArray(_tempVaos[gl]);
        }

        gl.BindBuffer(BufferTargetARB.ArrayBuffer, (uint)command.VertexBuffer.NativePointer);
        CheckErrors(_currentWindow.Gl, nameof(gl.BindBuffer));

        var offset = 0;
        for (uint i = 0; i < 16; i++)
        {
            if (i >= command.VertexBuffer.Format.Elements.Count)
            {
                gl.DisableVertexAttribArray(i);
                CheckErrors(_currentWindow.Gl, nameof(gl.DisableVertexAttribArray));
            }
            else
            {
                var element = command.VertexBuffer.Format.Elements[(int)i];

                gl.EnableVertexAttribArray(i);
                CheckErrors(_currentWindow.Gl, nameof(gl.EnableVertexAttribArray));

                gl.VertexAttribPointer(
                    i,
                    element.Format.ComponentsCount(),
                    element.Format.ToGl(),
                    element.Normalized,
                    (uint)command.VertexBuffer.Format.Stride,
                    offset
                );
                offset += element.Format.Size();
                CheckErrors(_currentWindow.Gl, nameof(gl.VertexAttribPointer));
            }
        }

        if (command.IndexBuffer is not null)
        {
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, (uint)command.IndexBuffer.NativePointer);
        }
        else
        {
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        }

        CheckErrors(_currentWindow.Gl, nameof(gl.BindBuffer));

        var fragSamplers = command.Material.FragmentStage.Samplers;
        for (int i = 0; i < fragSamplers.Length; i++)
        {
            var sampler = fragSamplers[i];
            if (sampler.Texture == null) continue; // todo: empty texture

            gl.ActiveTexture((TextureUnit)((uint)TextureUnit.Texture0 + (uint)i));
            CheckErrors(_currentWindow.Gl, nameof(gl.ActiveTexture));
            gl.BindTexture(TextureTarget.Texture2D, (uint)sampler.Texture.Handle);
            CheckErrors(_currentWindow.Gl, nameof(gl.BindTexture));

            // // Tell the shader that the sampler uniform should use this texture unit
            // int location = gl.GetUniformLocation(program, sampler.Name);
            // if (location != -1)
            //     gl.Uniform1(location, i);
        }

        if (command.IndexBuffer is not null)
        {
            var indexOffsetBytes = (IntPtr)(command.IndicesOffset * (int)command.IndexBuffer.IndexSize);
            gl.DrawElements(
                PrimitiveType.Triangles,
                (uint)command.IndicesCount,
                command.IndexBuffer.IndexSize == IndexSize._2
                    ? DrawElementsType.UnsignedShort
                    : DrawElementsType.UnsignedInt,
                indexOffsetBytes
            );
            CheckErrors(_currentWindow.Gl, nameof(gl.DrawElements));
        }
        else
        {
            gl.DrawArrays(
                PrimitiveType.Triangles,
                command.VerticesOffset,
                (uint)command.VerticesCount
            );
            CheckErrors(_currentWindow.Gl, nameof(gl.DrawArrays));
        }
    }

    public void Present()
    {
        if (_currentWindow is not null)
        {
            glPlatform.SwapBuffers(_currentWindow.Window);
        }

        _currentWindow = null;
    }

    public void Dispose()
    {
        foreach (var windowTarget in _windowTargets.Values)
        {
            windowTarget.Dispose();
        }

        _windowTargets.Clear();
        _currentWindow = null;
    }

    [MemberNotNull(nameof(_currentWindow))]
    private void CheckCurrentWindow()
    {
        if (_currentWindow is null)
        {
            throw new InvalidOperationException("First set render target");
        }
    }

    private uint GetOrCreateProgram(ShaderProgram vert, ShaderProgram frag)
    {
        var key = HashCode.Combine(vert.Handle, frag.Handle);
        if (_shadersCache.TryGetValue(key, out var prog)) return prog;

        CheckCurrentWindow();
        var gl = _currentWindow.Gl;

        var program = gl.CreateProgram();
        gl.AttachShader(program, (uint)vert.Handle);
        gl.AttachShader(program, (uint)frag.Handle);

        // gl.BindAttribLocation(program, 0, "aPosition");
        // gl.BindAttribLocation(program, 1, "aTexCoord");
        // gl.BindAttribLocation(program, 2, "aColor");

        gl.LinkProgram(program);
        gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int success);

        if (success == 0)
        {
            var log = gl.GetProgramInfoLog(program);
            gl.DeleteProgram(program);
            throw new InvalidOperationException($"GL program link failed:\n{log}");
        }

        gl.DetachShader(program, (uint)vert.Handle);
        gl.DetachShader(program, (uint)frag.Handle);

        return _shadersCache[key] = program;
    }


    private static BufferTargetARB GetTargetFor(GraphicBuffer buffer) => buffer.Usage == BufferUsage.Index
        ? BufferTargetARB.ElementArrayBuffer
        : BufferTargetARB.ArrayBuffer;
}
