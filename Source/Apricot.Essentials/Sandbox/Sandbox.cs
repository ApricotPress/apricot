using System.Numerics;
using Apricot.Assets;
using Apricot.Essentials.Bootstrap;
using Apricot.Essentials.DearImGui;
using Apricot.Graphics;
using Apricot.Graphics.Buffers;
using Apricot.Graphics.Commands;
using Apricot.Graphics.Materials;
using Apricot.Graphics.Shaders;
using Apricot.Graphics.Structs;
using Apricot.Platform;
using Apricot.Timing;
using Apricot.Windows;
using ImGuiNET;
using Microsoft.Extensions.Logging;

namespace Apricot.Essentials.Sandbox;

/// <summary>
/// Simple game used as a sandbox used while developing.
/// </summary>
public class SandboxGame(
    ITime time,
    IGraphics graphics,
    IWindowsManager windows,
    IAssetsDatabase assets,
    IPlatformInfo platform,
    ILogger<SandboxGame> logger
) : Game
{
    private readonly List<float> _lastDeltas = [];
    private readonly IRenderTarget _mainTarget = graphics.GetWindowRenderTarget(windows.GetOrCreateDefaultWindow());

    private float[] _samples = new float[256];
    private int _writeIndex;
    private double _lastTime = -1;

    // Layout one-time init
    private bool _first = true;

    private readonly ImGuiWindowRenderer _imGuiRenderer = new(
        graphics,
        windows.GetOrCreateDefaultWindow(),
        time,
        assets,
        platform
    );

    private readonly VertexBuffer<ImGuiVertex> _triangleVertices = graphics
        .CreateVertexBuffer<ImGuiVertex>(
            "triangle vertices",
            3
        );

    private readonly IndexBuffer _indices = graphics
        .CreateIndexBuffer(
            "indices",
            IndexSize._4,
            3
        );

    private readonly Material _mat = new(graphics.CreateShaderProgram(
        "Standard Vertex",
        new ShaderProgramDescription
        {
            Code = assets.GetArtifact(
                BuiltInAssets.Shaders.StandardVertex,
                new ArtifactTarget(platform.Platform, platform.GraphicDriver)
            ),
            EntryPoint = "vert",
            SamplerCount = 0,
            UniformBufferCount = 1,
            Stage = ShaderStage.Vertex
        }
    ), graphics.CreateShaderProgram(
        "Standard Fragment",
        new ShaderProgramDescription
        {
            Code = assets.GetArtifact(
                BuiltInAssets.Shaders.StandardFragment,
                new ArtifactTarget(platform.Platform, platform.GraphicDriver)
            ),
            EntryPoint = "frag",
            SamplerCount = 1,
            UniformBufferCount = 0,
            Stage = ShaderStage.Fragment
        }
    ));


    public override void Update()
    {
        _lastDeltas.Add(time.Delta);

        if (_lastDeltas.Count > 100)
        {
            _lastDeltas.RemoveAt(0);
        }

        ClearColor = Color.FromHsv(time.Time % 10f / 10f, 1, 0.5f);
    }


    public override void Render()
    {
        var t = time.Time;
        var mat =
            Matrix4x4.CreateScale(100, 100, 1.0f)
            * Matrix4x4.CreateTranslation(50, 50, 0)
            * Matrix4x4.CreateOrthographicOffCenter(0, _mainTarget.Width, _mainTarget.Height, 0, 0.1f, 1000.0f);
        _indices.UploadData([0, 1, 2]);

        _mat.VertexStage.SetUniformBuffer(mat);
        _mat.FragmentStage.Samplers[0] = new BoundSampler(
            graphics.EmptyTexture,
            new TextureSampler()
        );
        _triangleVertices.UploadData([
            new ImGuiVertex(new Vector2(-1, -1), new Vector2(-1, -1), Color.Red),
            new ImGuiVertex(new Vector2(1, -1), new Vector2(1, -1), Color.White),
            new ImGuiVertex(new Vector2(0, 1), new Vector2(0, 1), Color.Blue),
        ]);

        graphics.Submit(new DrawCommand(_mainTarget, _mat, _triangleVertices)
        {
            IndexBuffer = _indices,
            IndicesCount = 3,
            VerticesCount = 3
        });

        _imGuiRenderer.Begin();
        _lastTime = t;

        // Animated value in [0..1]
        float v = 0.5f + 0.5f * (float)Math.Sin(t * 1.6f);

        // Update sparkline
        _samples[_writeIndex++ % _samples.Length] = v;

        // --- window setup (non-interactable) ---
        if (_first)
        {
            ImGui.SetNextWindowSize(new Vector2(720, 460), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowPos(new Vector2(40, 40), ImGuiCond.FirstUseEver);
            _first = false;
        }

        const ImGuiWindowFlags flags =
            ImGuiWindowFlags.NoCollapse |
            ImGuiWindowFlags.NoSavedSettings |
            ImGuiWindowFlags.NoBringToFrontOnFocus |
            ImGuiWindowFlags.NoInputs; // <- ignore all mouse/keyboard

        if (ImGui.Begin("Non-Interactive Dashboard", flags))
        {
            // Also gray out everything visually
            ImGui.BeginDisabled(true);

            // Header
            ImGui.TextColored(new Vector4(0.9f, 0.9f, 1.0f, 1), "Status Overview");
            ImGui.SameLine();
            ImGui.TextDisabled($"  (t = {t:0.00}s)");

            ImGui.Separator();

            // --- top row: animated progress + radial spinner ---
            {
                ImGui.Text("Throughput");
                ImGui.ProgressBar(v, new Vector2(-1, 0), $"{v * 100f:0}%");

                ImGui.SameLine();

                // Rotating arc "spinner" using the window draw list
                var dl = ImGui.GetWindowDrawList();
                Vector2 p = ImGui.GetCursorScreenPos();
                float size = 46;
                Vector2 center = new Vector2(p.X + size, p.Y + size * 0.9f);
                float radius = 18.0f;
                float a0 = (float)t * 2.6f;
                float a1 = a0 + 2.2f;
                uint col = ImGui.ColorConvertFloat4ToU32(new Vector4(0.5f, 0.8f, 1.0f, 1.0f));
                dl.PathArcTo(center, radius, a0, a1, 32);
                dl.PathStroke(col, ImDrawFlags.None, 4.0f);
                ImGui.Dummy(new Vector2(size * 2, size)); // reserve space
            }

            ImGui.Spacing();
            ImGui.Separator();

            // --- middle row: metrics as a table ---
            if (ImGui.BeginTable("metrics", 3,
                    ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchProp))
            {
                ImGui.TableSetupColumn("Metric");
                ImGui.TableSetupColumn("Value");
                ImGui.TableSetupColumn("Trend");
                ImGui.TableHeadersRow();

                AddMetricRow("FPS", $"{ImGui.GetIO().Framerate:0}", (float)Math.Abs(Math.Sin(t * 0.7)));
                AddMetricRow("Latency (ms)", $"{(18 + 10 * (1 - v)):0.0}", 1 - v);
                AddMetricRow("Load (%)", $"{(35 + 60 * v):0}", v);

                ImGui.EndTable();
            }

            ImGui.Spacing();

            // --- sparkline / tiny plot (using PlotLines) ---
            ImGui.Text("Signal");
            unsafe
            {
                fixed (float* ptr = &_samples[0])
                {
                    ImGui.PlotLines(
                        "##",
                        ref _samples[0],
                        _samples.Length,
                        _writeIndex,
                        "animated sine",
                        0,
                        1,
                        new Vector2(-1, 80)
                    );
                }
            }

            ImGui.Spacing();
            ImGui.Separator();

            // --- "rich" widgets (but all disabled & non-interactable) ---
            ImGui.Text("Controls (display-only)");
            ImGui.Columns(3, "cols", false);

            // Col 1
            ImGui.Text("Buttons");
            ImGui.Button("Run");
            ImGui.SameLine();
            ImGui.Button("Stop");

            // Col 2
            ImGui.NextColumn();
            ImGui.Text("Sliders / Inputs");
            float fake = 42.0f + 10.0f * (float)Math.Sin(t * 0.9f);
            ImGui.SliderFloat("Rate", ref fake, 0, 100);
            int fakesteps = (int)(3 + 2 * Math.Abs(Math.Sin(t * 0.5)));
            ImGui.SliderInt("Steps", ref fakesteps, 0, 10);

            // Col 3
            ImGui.NextColumn();
            ImGui.Text("Toggles / Color");
            bool fakeToggle = (Math.Sin(t) > 0);
            ImGui.Checkbox("Enabled", ref fakeToggle);
            var colVec = new Vector4(0.1f + 0.9f * v, 0.4f, 0.9f - 0.9f * v, 1);
            ImGui.ColorEdit4("Tint", ref colVec, ImGuiColorEditFlags.NoInputs);

            ImGui.Columns(1);

            ImGui.EndDisabled();
        }

        ImGui.End();
        _imGuiRenderer.End();
    }

    private static void AddMetricRow(string name, string value, float trend01)
    {
        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        ImGui.TextUnformatted(name);

        ImGui.TableNextColumn();
        ImGui.TextColored(new Vector4(0.85f, 1f, 0.85f, 1), value);

        ImGui.TableNextColumn();
        float w = MathF.Max(60, ImGui.GetContentRegionAvail().X);
        ImGui.ProgressBar(trend01, new Vector2(w, 0));
    }
}
