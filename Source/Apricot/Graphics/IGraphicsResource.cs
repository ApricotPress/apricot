namespace Apricot.Graphics;

/// <summary>
/// Some resource bound to <see cref="IGraphics"/>.
/// </summary>
public interface IGraphicsResource : IDisposable
{
    /// <summary>
    /// Name used for debug purposes.
    /// </summary>
    string Name { get; }
    
    bool IsDisposed { get; }
}
