using System.Numerics;
using Apricot.Graphics;
using Apricot.Lifecycle.TickHandlers;
using Apricot.Timing;
using ImGuiNET;
using Microsoft.Extensions.Logging;

namespace Apricot.Essentials.Sandbox;

public class Sandbox(
    ITime time,
    IGraphics graphics,
    ILogger<Sandbox> logger
) : IUpdateHandler, IDrawHandler
{
    private readonly List<float> _lastDeltas = [];

    public void Update()
    {
        _lastDeltas.Add(time.Delta);

        if (_lastDeltas.Count > 100)
        {
            _lastDeltas.RemoveAt(0);
        }


        logger.LogInformation("Average FPS: {Average:F}", 1f / _lastDeltas.Average());
    }

    public void Draw()
    {
        var t = time.Time % 10f / 10f;
        var color = Color.FromHsv(t, 1, 0.5f);

        graphics.Clear(color);
        
        ImGui.Text("Hello, world!");
        ImGui.Dummy(new Vector2(10, 10));
    }
}
