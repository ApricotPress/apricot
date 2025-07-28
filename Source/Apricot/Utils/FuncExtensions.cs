namespace Apricot.Utils;

public static class FuncExtensions
{
    public static T DoIf<T>(this T chain, bool condition, Func<T, T> link) => condition
        ? link(chain)
        : chain;
}
