namespace Apricot;

public interface ISubsystem
{
    void Initialize(App app);
    
    void BeforeTick() { }
    
    void AfterTick() { }
    
    void Quit() { }
}
