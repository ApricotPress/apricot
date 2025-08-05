using Apricot.Lifecycle.TickHandlers;

namespace Apricot.Lifecycle;

public record struct GameLoop(
    string Identifier,
    GameLoop[] ChildGameLoops,
    ITickHandler? Handler = null
)
{
    public static GameLoop SimpleHandler(string id, ITickHandler handler) => new(id, [], handler);
}
