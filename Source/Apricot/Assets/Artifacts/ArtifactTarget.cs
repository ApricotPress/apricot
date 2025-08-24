using Apricot.Graphics;
using Apricot.Platform;
using MessagePack;

namespace Apricot.Assets.Artifacts;

/// <summary>
/// Struct representing what platforms artifact supports. Null value (or empty arrays) of param represents wildcard.
/// </summary>
/// <param name="Platform">Target runtime platform that artifact is suitable for.</param>
/// <param name="GraphicDriver">Target graphic driver that artifact is suitable for.</param>
/// <param name="Tags">List of tags produced on asset import.</param>
[MessagePackObject(true)]
public readonly record struct ArtifactTarget(
    RuntimePlatform? Platform,
    GraphicDriver? GraphicDriver,
    string[] Tags
)
{
    /// <summary>
    /// Check whether target matches query. To match platform and driver should be either null in query or target or
    /// equal. For tags it would check that each tag from query exists in current target. 
    /// </summary>
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
