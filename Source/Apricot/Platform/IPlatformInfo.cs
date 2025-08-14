namespace Apricot.Platform;

public interface IPlatformInfo
{
    RuntimePlatform Platform { get; }
    
    GraphicDriver GraphicDriver { get; }
}
