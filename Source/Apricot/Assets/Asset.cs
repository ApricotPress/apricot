namespace Apricot.Assets;

/// <summary>
/// Represents info about asset stored in <see cref="IAssetDatabase"/>. 
/// </summary>
public record Asset(
    string Name,
    Guid Id,
    Uri Uri
)
{
    public override string ToString() => $"Asset<{Name}, {Id}, {Uri}>";
}
