using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;
using Apricot.Essentials.Assets;
using Apricot.Graphics;
using Apricot.Graphics.Buffers;
using Apricot.Graphics.Commands;
using Apricot.Graphics.Materials;
using Apricot.Graphics.Shaders;
using Apricot.Graphics.Textures;
using Apricot.Lifecycle.TickHandlers;
using Apricot.Resources;
using Apricot.Timing;
using Apricot.Windows;
using ImGuiNET;

namespace Apricot.Essentials.DearImGui;

/// <summary>
/// Class with <see cref="ITickHandler">tick handlers</see> that should be called before and after rendering to render
/// ImGui. Designed to work with a single window.
/// </summary>
/// <remarks>
/// Renderer assumes that resources and graphics were already initialized and would try to instantly create shaders and
/// required resources.
/// </remarks>
public sealed unsafe class ImGuiWindowRenderer
{
    private readonly IntPtr _imGuiContext;

    private readonly IGraphics _graphics;
    private readonly IWindow _window;
    private readonly ITime _time;

    private int _texturesCount;
    private IntPtr? _fontTextureId;
    private readonly Dictionary<IntPtr, Texture> _loadedTextures = [];

    private VertexBuffer<ImGuiVertex> _vertexBuffer;
    private IndexBuffer _indexBuffer;

    private ImGuiVertex[] _vertices;
    private ushort[] _indices;

    private readonly IRenderTarget _renderTarget;
    private readonly Material _material;

    public ImGuiWindowRenderer(
        IGraphics graphics,
        IWindow window,
        ITime time,
        IResourcesLoader resources
    )
    {
        _imGuiContext = ImGui.CreateContext();

        _graphics = graphics;
        _window = window;
        _time = time;

        _renderTarget = graphics.GetWindowRenderTarget(window);
        _material = new Material(
            resources.Load<ShaderProgram>(EssentialsIds.Shaders.StandardVertex),
            resources.Load<ShaderProgram>(EssentialsIds.Shaders.StandardFragment)
        );

        ResizeVertexBuffer(2048);
        ResizeIndexBuffer(1024);
        RebuildFontAtlas();
    }

    public void Begin()
    {
        ImGui.SetCurrentContext(_imGuiContext);

        var io = ImGui.GetIO();

        io.DeltaTime = _time.Delta;
        io.DisplaySize = new Vector2(_window.Width, _window.Height);
        io.DisplayFramebufferScale = Vector2.One;

        ImGui.NewFrame();
    }

    public void End()
    {
        ImGui.Render();

        var data = ImGui.GetDrawData();

        Render(data);

        ImGui.SetCurrentContext(IntPtr.Zero);
    }

    /// <summary>
    /// Rebuilds font atlas and uploads it to GPU.
    /// </summary>
    public void RebuildFontAtlas()
    {
        var io = ImGui.GetIO();
        io.Fonts.GetTexDataAsRGBA32(out byte* pixelData, out var width, out var height, out var bytesPerPixel);

        var pixels = new byte[width * height * bytesPerPixel];
        Marshal.Copy(new IntPtr(pixelData), pixels, 0, pixels.Length);

        var texture = _graphics.CreateTexture("ImGUI font", width, height);
        texture.SetData(pixels);

        if (_fontTextureId.HasValue) UnbindTexture(_fontTextureId.Value);

        _fontTextureId = BindTexture(texture);

        io.Fonts.SetTexID(_fontTextureId.Value);
        io.Fonts.ClearTexData();
    }

    private IntPtr BindTexture(Texture texture)
    {
        var id = new IntPtr(_texturesCount++);

        _loadedTextures.Add(id, texture);

        return id;
    }

    private void UnbindTexture(IntPtr textureId)
    {
        _loadedTextures.Remove(textureId);
    }

    private void Render(ImDrawDataPtr data)
    {
        RebuildBuffers(data);

        var cmd = new DrawCommand(_renderTarget, _material, _vertexBuffer)
        {
            BlendMode = new BlendMode(BlendOp.Add, BlendFactor.SrcAlpha, BlendFactor.OneMinusSrcAlpha),
            IndexBuffer = _indexBuffer
        };

        var mat =
            Matrix4x4.CreateScale(data.FramebufferScale.X, data.FramebufferScale.Y, 1.0f) *
            Matrix4x4.CreateOrthographicOffCenter(0, _window.Width, _window.Height, 0, 0.1f, 1000.0f);
        _material.VertexStage.SetUniformBuffer(mat);

        var globalVtxOffset = 0;
        var globalIdxOffset = 0;
        for (var i = 0; i < data.CmdListsCount; i++)
        {
            var imList = data.CmdLists[i];

            for (var j = 0; j < imList.CmdBuffer.Size; j++)
            {
                var imCmd = imList.CmdBuffer[j];

                var textureIndex = imCmd.TextureId.ToInt32();
                if (textureIndex < _loadedTextures.Count)
                    _material.FragmentStage.Samplers[0] = new BoundSampler(
                        _loadedTextures[textureIndex],
                        new TextureSampler()
                    );

                cmd.VerticesOffset = (int)(imCmd.VtxOffset + globalVtxOffset);
                cmd.IndicesOffset = (int)(imCmd.IdxOffset + globalIdxOffset);
                cmd.IndicesCount = (int)imCmd.ElemCount;

                _graphics.Submit(cmd);
            }

            globalVtxOffset += imList.VtxBuffer.Size;
            globalIdxOffset += imList.IdxBuffer.Size;
        }
    }

    private void RebuildBuffers(ImDrawDataPtr data)
    {
        if (data.TotalVtxCount <= 0) return;

        if (data.TotalVtxCount > _vertexBuffer.Capacity)
        {
            ResizeVertexBuffer((int)(data.TotalVtxCount * 1.5f));
        }

        if (data.TotalIdxCount > _indexBuffer.Capacity)
        {
            ResizeIndexBuffer((int)(data.TotalIdxCount * 1.5f));
        }

        var vtxOffset = 0;
        var idxOffset = 0;

        for (var i = 0; i < data.CmdListsCount; i++)
        {
            var list = data.CmdLists[i];

            var vertexSrc = new Span<ImGuiVertex>(list.VtxBuffer.Data.ToPointer(), list.VtxBuffer.Size);
            var indexSrc = new Span<ushort>(list.IdxBuffer.Data.ToPointer(), list.IdxBuffer.Size);

            vertexSrc.CopyTo(_vertices.AsSpan()[vtxOffset..]);
            indexSrc.CopyTo(_indices.AsSpan()[idxOffset..]);

            vtxOffset += list.VtxBuffer.Size;
            idxOffset += list.IdxBuffer.Size;
        }

        _vertexBuffer.UploadData(_vertices);
        _indexBuffer.UploadData(_indices);
    }

    [MemberNotNull(nameof(_vertexBuffer)), MemberNotNull(nameof(_vertices))]
    private void ResizeVertexBuffer(int capacity)
    {
        _vertexBuffer?.Dispose();

        Array.Resize(ref _vertices, capacity);
        _vertexBuffer = _graphics.CreateVertexBuffer<ImGuiVertex>(
            "ImGui vertices",
            capacity
        );
    }

    [MemberNotNull(nameof(_indexBuffer)), MemberNotNull(nameof(_indices))]
    private void ResizeIndexBuffer(int capacity)
    {
        _indexBuffer?.Dispose();

        Array.Resize(ref _indices, capacity);
        _indexBuffer = _graphics.CreateIndexBuffer(
            "ImGui indices",
            IndexSize._2,
            capacity
        );
    }
}
