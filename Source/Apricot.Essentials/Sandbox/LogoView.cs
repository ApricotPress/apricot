using System.Numerics;
using Apricot.Essentials.Assets;
using Apricot.Essentials.Bootstrap;
using Apricot.Essentials.DearImGui;
using Apricot.Graphics;
using Apricot.Graphics.Buffers;
using Apricot.Graphics.Commands;
using Apricot.Graphics.Materials;
using Apricot.Graphics.Shaders;
using Apricot.Graphics.Structs;
using Apricot.Graphics.Textures;
using Apricot.Resources;
using Apricot.Windows;

namespace Apricot.Essentials.Sandbox;

/// <summary>
/// Game that just shows image with logo at center of the screen.
/// </summary>
public class LogoView(
    IGraphics graphics,
    IWindowsManager windows,
    IResourcesLoader resources
) : Game
{
    private readonly IRenderTarget _mainTarget = graphics.GetWindowRenderTarget(windows.GetOrCreateDefaultWindow());

    private readonly VertexBuffer<ImGuiVertex> _vertices = graphics
        .CreateVertexBuffer<ImGuiVertex>(
            "quad vertices",
            4
        );

    private readonly Texture _logo = resources.Load<Texture>(EssentialsIds.Textures.ApricotLogo);

    private readonly IndexBuffer _indices = graphics
        .CreateIndexBuffer(
            "indices",
            IndexSize._4,
            6
        );

    private readonly Material _mat = new(
        resources.Load<ShaderProgram>(EssentialsIds.Shaders.StandardVertex),
        resources.Load<ShaderProgram>(EssentialsIds.Shaders.StandardFragment)
    );

    public override void Update() => ClearColor = new PackedColor(255, 217, 132, 255);


    public override void Render()
    {
        var mat =
            Matrix4x4.CreateScale(300, 300, 1.0f)
            * Matrix4x4.CreateTranslation(_mainTarget.Width / 2f, _mainTarget.Height / 2f, 0)
            * Matrix4x4.CreateOrthographicOffCenter(0, _mainTarget.Width, _mainTarget.Height, 0, 0.1f, 1000.0f);
        _indices.UploadData([0, 1, 2, 0, 2, 3]);

        _mat.VertexStage.SetUniformBuffer(mat);
        _mat.FragmentStage.Samplers[0] = new BoundSampler(
            _logo,
            new TextureSampler()
        );
        _vertices.UploadData([
            new ImGuiVertex(new Vector2(-1, -1), new Vector2(0, 0), Color.White),
            new ImGuiVertex(new Vector2(-1, 1), new Vector2(0, 1), Color.White),
            new ImGuiVertex(new Vector2(1, 1), new Vector2(1, 1), Color.White),
            new ImGuiVertex(new Vector2(1, -1), new Vector2(1, 0), Color.White),
        ]);

        graphics.Submit(new DrawCommand(_mainTarget, _mat, _vertices)
        {
            IndexBuffer = _indices,
            IndicesCount = _indices.Capacity,
            VerticesCount = _vertices.Capacity
        });
    }
}
