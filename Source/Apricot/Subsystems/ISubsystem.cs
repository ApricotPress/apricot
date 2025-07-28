namespace Apricot.Subsystems;

public interface ISubsystem
{
    void Initialize(App app);
    
    void BeforeTick() { }
    
    void AfterTick() { }
}
