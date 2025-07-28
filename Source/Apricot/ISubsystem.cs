namespace Apricot;

/// <summary>
/// Subsystem of engine that has callbacks for essential game engine lifecycle. 
/// </summary>
public interface ISubsystem
{
    void Initialize() { }

    /// <summary>
    /// Called from main thread by <see cref="Jar.Tick"/> before processing
    /// <see cref="Apricot.Scheduling.IMainThreadScheduler"/> queue.
    /// </summary>
    void BeforeFrame() { }

    /// <summary>
    /// Called from main thread by <see cref="Jar.Tick"/> after finishing processing
    /// <see cref="Apricot.Scheduling.IMainThreadScheduler"/> queue.
    /// </summary>
    void AfterFrame() { }

    /// <summary>
    /// Called from <see cref="Jar.AfterRun"/> to gracefully uninitialize subsystem.
    /// </summary>
    void Quit() { }
}
