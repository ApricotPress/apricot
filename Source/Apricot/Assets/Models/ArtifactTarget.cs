using Apricot.Graphics;
using Apricot.Platform;

namespace Apricot.Assets;

/// <summary>
/// Struct representing what platforms artifact supports. Nul lvalue of param represents wildcard.
/// </summary>
/// <param name="Platform">Target runtime platform that artifact is suitable for.</param>
/// <param name="GraphicDriver">Target graphic driver that artifact is suitable for.</param>
public readonly record struct ArtifactTarget(
    RuntimePlatform? Platform,
    GraphicDriver? GraphicDriver
)
{
    public bool Matches(ArtifactTarget query)
    {
        var platformMatch =
            query.Platform is null
            || Platform is null
            || Platform == query.Platform.Value;
        var graphicsMatch =
            query.GraphicDriver is null
            || GraphicDriver is null
            || GraphicDriver == query.GraphicDriver.Value;

        return platformMatch && graphicsMatch;
    }
}
