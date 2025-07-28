namespace Apricot;

public interface ISubsystem
{
    void Initialize(Jar jar);

    void BeforeFrame();
    
    void Quit() { }
}
