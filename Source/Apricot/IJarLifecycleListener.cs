namespace Apricot;

/// <summary>
/// Listens to lifecycle of jar.
/// </summary>
public interface IJarLifecycleListener
{
    /// <summary>
    /// Called from main thread before actual jar initialization and main
    /// <see cref="Apricot.Windows.IWindow"/> is created.
    /// </summary>
    /// <seealso cref="Apricot.Jar.Init"/>
    void OnBeforeInitialization() { }

    /// <summary>
    /// Called from main thread after actual jar initialization and main
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
