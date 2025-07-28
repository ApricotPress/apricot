namespace Apricot.Events;

public interface IJarLifecycleListener
{
    void OnBeforeInitialization() { }

    void OnAfterInitialization() {}

    void OnBeforeQuit() { }
}
