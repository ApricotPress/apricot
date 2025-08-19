using Apricot.Graphics;

namespace Apricot;

/// <summary>
/// Generic of Jar. 
/// </summary>
public class JarOptions
{
    /// <summary>
    /// Preferred driver that should be used if available. Ignored if not supported by <see cref="IGraphics"/>.
    /// </summary>
    public GraphicDriver PreferredDriver { get; set; }

    /// <summary>
    /// Should some additional debug options be passed to graphic API by <see cref="IGraphics"/>.
    /// </summary>
    public bool EnableGraphicsDebug { get; set; }
}
