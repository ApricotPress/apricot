namespace Apricot.Utils;

public static class FuncExtensions
{
    /// <summary>
    /// Calls <see cref="link"/> with <see cref="chain"/> as argument if <see cref="condition"/> is true. Returns chain
    /// itself otherwise. 
    /// </summary>
    /// <param name="chain">Some chainable object.</param>
    /// <param name="condition">Whether return <see cref="link"/> result or <see cref="chain"/>.</param>
    /// <param name="link">Operation to perfom on chainable object.</param>
    /// <typeparam name="T">Type of chain, can be basically anything.</typeparam>
    /// <returns>Result of conditional link operation or original chain.</returns>
    public static T DoIf<T>(this T chain, bool condition, Func<T, T> link) => condition
        ? link(chain)
        : chain;
}
