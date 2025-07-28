namespace Apricot;

public interface ISubsystem
{
    void Initialize(App app);

    void BeforeFrame();
    
    void Quit() { }
}
