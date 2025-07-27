namespace Apricot.Scheduling;

public interface IAppLifecycleListener
{
    void OnBeforeInitialization();

    void OnAfterInitialization();
}
