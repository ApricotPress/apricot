using System.Numerics;
using System.Runtime.InteropServices;
using Apricot.Graphics;
using Apricot.Graphics.Resources;
using Apricot.Lifecycle.TickHandlers;
using Apricot.Timing;
using Apricot.Windows;
using ImGuiNET;

namespace Apricot.Extensions;

public unsafe class ImGuiWindowRenderer
{
    private readonly IntPtr _imGuiContext;

    private readonly IGraphics _graphics;
    private readonly IWindow _window;
    private readonly ITime _time;

    private int _texturesCount;
    private IntPtr? _fontTextureId;
    private readonly Dictionary<IntPtr, Texture> _loadedTextures = [];

    public ITickHandler BeginLayout { get; }

    public ITickHandler EndLayout { get; }

    public ImGuiWindowRenderer(IGraphics graphics, IWindow window, ITime time)
    {
        _imGuiContext = ImGui.CreateContext();

        _graphics = graphics;
        _window = window;
        _time = time;

        BeginLayout = new BeginHandler(this);
        EndLayout = new EndHandler(this);
    }
    
    public void RebuildFontAtlas()
    {
        var io = ImGui.GetIO();
        io.Fonts.GetTexDataAsRGBA32(out byte* pixelData, out var width, out var height, out var bytesPerPixel);

        var pixels = new byte[width * height * bytesPerPixel];
        Marshal.Copy(new IntPtr(pixelData), pixels, 0, pixels.Length);

        var texture = _graphics.CreateTexture("ImGUI Font", width, height);
        texture.SetData(pixels);

        if (_fontTextureId.HasValue) UnbindTexture(_fontTextureId.Value);

        _fontTextureId = BindTexture(texture);

        io.Fonts.SetTexID(_fontTextureId.Value);
        io.Fonts.ClearTexData();
    }
    
    public virtual IntPtr BindTexture(Texture texture)
    {
        var id = new IntPtr(_texturesCount++);

        _loadedTextures.Add(id, texture);

        return id;
    }

    public virtual void UnbindTexture(IntPtr textureId)
    {
        _loadedTextures.Remove(textureId);
    }

    private class BeginHandler(ImGuiWindowRenderer renderer) : ITickHandler
    {
        public void Tick()
        {
            ImGui.SetCurrentContext(renderer._imGuiContext);

            var io = ImGui.GetIO();
            var window = renderer._window;

            io.DeltaTime = renderer._time.Delta;
            io.DisplaySize = new Vector2(window.Width, window.Height);
            io.DisplayFramebufferScale = Vector2.One;
            
            ImGui.NewFrame();
        }
    }
    
    private class EndHandler(ImGuiWindowRenderer renderer) : ITickHandler
    {
        public void Tick()
        {
            ImGui.Render();

            var data = ImGui.GetDrawData();
            ImGui.SetCurrentContext(IntPtr.Zero);
        }
    }
}
