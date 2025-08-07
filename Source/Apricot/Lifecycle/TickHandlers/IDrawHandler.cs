namespace Apricot.Lifecycle.TickHandlers;

/// <summary>
/// Handler that automatically being added to render section of <see cref="GameLoop"/> by
/// <see cref="DefaultGameLoopProvider"/>.
/// </summary>
/// <seealso cref="Ids.Render"/>
public interface IDrawHandler
{
    void Draw();
}
