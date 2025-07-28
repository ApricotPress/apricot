namespace Apricot.Events;

public interface IAppLifecycleListener
{
    void OnBeforeInitialization() { }

    void OnAfterInitialization() {}

    void OnBeforeQuit() { }
}
