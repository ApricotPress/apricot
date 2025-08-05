namespace Apricot.Lifecycle.TickHandlers;

/// <summary>
/// A class that is passed to <see cref="GameLoop"/> to be executed every frame.
///
/// See default tick handlers in namespace for basic injection points when using <see cref="DefaultGameLoopProvider"/>.
/// </summary>
public interface ITickHandler
{
    /// <summary>
    /// Body that would be executed inside of <see cref="Jar.Tick"/>.
    /// </summary>
    void Tick();
}
