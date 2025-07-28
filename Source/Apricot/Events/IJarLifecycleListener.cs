namespace Apricot.Events;

/// <summary>
/// Listens to lifecycle of jar.
/// </summary>
public interface IJarLifecycleListener
{
    /// <summary>
    /// Called from main thread before subsystems initialization is started and main
    /// <see cref="Apricot.Windows.IWindow"/> is created.
    /// </summary>
    /// <seealso cref="Apricot.Jar.Init"/>
    void OnBeforeInitialization() { }


    /// <summary>
    /// Called from main thread after all subsystems finished their initialization and main
    /// <see cref="Apricot.Windows.IWindow"/> is created.
    /// </summary>
    /// <seealso cref="Apricot.Jar.Init"/>
    void OnAfterInitialization() { }

    /// <summary>
    /// Called from main thread after game loop is finished processing last frame. 
    /// </summary>
    /// <seealso cref="Apricot.Jar.Run"/>
    void OnBeforeQuit() { }
}
