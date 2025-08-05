using Apricot.Lifecycle.TickHandlers;

namespace Apricot.Timing;

/// <summary>
/// Controls <see cref="ITime"/>. Should be used inside game loop and to react to app lifecycle to pause/resume.  
/// </summary>
public interface ITimeController : ITickHandler;
