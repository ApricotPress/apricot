namespace Apricot.Lifecycle.TickHandlers;

/// <summary>
/// Handler that automatically being added to update section of <see cref="GameLoop"/> by
/// <see cref="DefaultGameLoopProvider"/>.
/// </summary>
/// <seealso cref="Ids.Update"/>
public interface IUpdateHandler
{
    void Update();
}
