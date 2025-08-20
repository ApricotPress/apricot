using Apricot.Lifecycle.TickHandlers;

namespace Apricot.Lifecycle;

/// <summary>
/// Recursive structure representing game loop or its inner sub-game loop parts. It is built in similar to Unity's
/// PlayerLoop manner.
/// </summary>
/// <param name="Identifier">Name of game-loop part that is used for Debug or modifying purposes.</param>
/// <param name="ChildGameLoops">Child game loops. They would be called BEFORE <see cref="Handler"/> if last one was provided.</param>
/// <param name="Handler">Handler that has Tick method that would be called after all children game loops were processed.</param>
public record struct GameLoop(
    string Identifier,
    GameLoop[] ChildGameLoops,
    ITickHandler? Handler = null
)
{
    public static GameLoop SimpleHandler(string id, ITickHandler? handler) => new(id, [], handler);

    public override string ToString()
    {
        var handlerStar = Handler is null ? string.Empty : "*";
        return $"GameLoop {Identifier}[{ChildGameLoops.Length}]{handlerStar}";
    }
}
