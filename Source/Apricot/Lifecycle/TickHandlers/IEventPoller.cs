using Apricot.Lifecycle;

namespace Apricot.Events;

/// <summary>
/// Handler that automatically being added to pre-update section of <see cref="GameLoop"/> by
/// <see cref="DefaultGameLoopProvider"/> for polling events from OS.
/// </summary>
/// <seealso cref="Ids.EventPolling"/>
public interface IEventPoller
{
    void Poll();
}
