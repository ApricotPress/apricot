namespace Apricot.Lifecycle.TickHandlers;

/// <summary>
/// Tick handler that automatically being added to update section of <see cref="GameLoop"/> by <see cref="DefaultGameLoopProvider"/>.
/// </summary>
public interface IUpdateHandler : ITickHandler;
