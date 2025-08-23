using Apricot.Graphics;
using Apricot.Platform;
using MessagePack;

namespace Apricot.Assets.Artifacts;

/// <summary>
/// Struct representing what platforms artifact supports. Nul lvalue of param represents wildcard.
/// </summary>
/// <param name="Platform">Target runtime platform that artifact is suitable for.</param>
/// <param name="GraphicDriver">Target graphic driver that artifact is suitable for.</param>
[MessagePackObject(true)]
public readonly record struct ArtifactTarget(
    RuntimePlatform? Platform,
    GraphicDriver? GraphicDriver,
    string[] Tags
)
{
    public ArtifactTarget(RuntimePlatform? platform, GraphicDriver? driver) : this(platform, driver, []) { }

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
        var tagsMatch = query.Tags.All(Tags.Contains);

        return platformMatch && graphicsMatch && tagsMatch;
    }
}
