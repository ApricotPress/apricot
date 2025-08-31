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
public class LogoView : Game
{
    private readonly IGraphics _graphics;
    private readonly DrawCommand _drawLogoCommand;

    public LogoView(
        IGraphics graphics,
        IWindowsManager windows,
        IResourcesLoader resources
    )
    {
        _graphics = graphics;
        var mainTarget = graphics.GetWindowRenderTarget(windows.GetOrCreateDefaultWindow());

        var vertices = graphics
            .CreateVertexBuffer<ImGuiVertex>(
                "quad vertices",
                4
            );
        vertices.UploadData([
            new ImGuiVertex(new Vector2(-1, -1), new Vector2(0, 0), Color.White),
            new ImGuiVertex(new Vector2(-1, 1), new Vector2(0, 1), Color.White),
            new ImGuiVertex(new Vector2(1, 1), new Vector2(1, 1), Color.White),
            new ImGuiVertex(new Vector2(1, -1), new Vector2(1, 0), Color.White),
        ]);

        var indices = graphics
            .CreateIndexBuffer(
                "indices",
                IndexSize._4,
                6
            );
        indices.UploadData([0, 1, 2, 0, 2, 3]);

        var mat = new Material(
            resources.Load<ShaderProgram>(EssentialsIds.Shaders.StandardVertex),
            resources.Load<ShaderProgram>(EssentialsIds.Shaders.StandardFragment)
        );

        mat.VertexStage.SetUniformBuffer(
            Matrix4x4.CreateScale(300, 300, 1.0f)
            * Matrix4x4.CreateTranslation(mainTarget.Width / 2f, mainTarget.Height / 2f, 0)
            * Matrix4x4.CreateOrthographicOffCenter(0, mainTarget.Width, mainTarget.Height, 0, 0.1f, 1000.0f)
        );
        mat.FragmentStage.Samplers[0] = new BoundSampler(
            resources.Load<Texture>(EssentialsIds.Textures.ApricotLogo),
            new TextureSampler()
        );
        _drawLogoCommand = new DrawCommand(mainTarget, mat, vertices)
        {
            IndexBuffer = indices,
            IndicesCount = indices.Capacity,
            VerticesCount = vertices.Capacity
        };
    }

    public override void Update() => ClearColor = new PackedColor(255, 217, 132, 255);


    public override void Render() => _graphics.Submit(_drawLogoCommand);
}
