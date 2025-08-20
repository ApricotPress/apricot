using Apricot.Graphics.Structs;
using Apricot.Lifecycle;

namespace Apricot.Essentials.Bootstrap;

/// <summary>
/// This is a basic class to use with <see cref="GameJar{TGame}"/>. Read more in GameJar docs, but tl;dr: it is the
/// fasters way to create a game.
/// </summary>
public abstract class Game
{
    /// <summary>
    /// Color that window would be cleared with automatically on render phase.
    /// </summary>
    public Color ClearColor { get; set; } = Color.White;

    /// <summary>
    /// Update method called in <see cref="Ids.Update"/> phase of game loop by <see cref="GameJar{TGame}"/>.
    /// </summary>
    public abstract void Update();
    
    /// <summary>
    /// Render method called in <see cref="Ids.Render"/> phase of game loop by <see cref="GameJar{TGame}"/>.
    /// </summary>
    public abstract void Render();
}
