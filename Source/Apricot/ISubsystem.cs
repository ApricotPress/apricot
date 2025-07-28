namespace Apricot;

public interface ISubsystem
{
    void Initialize(App app);

    void ScheduleFrame();
    
    void Quit() { }
}
