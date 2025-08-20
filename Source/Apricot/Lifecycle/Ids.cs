namespace Apricot.Lifecycle;

/// <summary>
/// Ids for default <see cref="GameLoop"/> implementation used by <see cref="DefaultGameLoopProvider"/>.
/// </summary>
public static class Ids
{
    public const string DefaultGameLoop = nameof(DefaultGameLoop);

    public const string PreUpdate = nameof(PreUpdate);
    public const string Time = nameof(Time);
    public const string EventPolling = nameof(EventPolling);


    public const string Update = nameof(Update);
    public const string GenricUpdateHandlers = nameof(GenricUpdateHandlers);


    public const string Render = nameof(Render);
    public const string GenericRenderHandlers = nameof(GenericRenderHandlers);
}
