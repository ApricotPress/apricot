namespace Apricot.Resources;

/// <summary>
/// Interface of generic factory that can produce resource of type <typeparamref name="T"/> from given
/// <typeparamref name="TArg"/> argument.
/// </summary>
/// <typeparam name="T">Resource type it produces.</typeparam>
/// <typeparam name="TArg">Generic argument factory uses to produce resource.</typeparam>
public interface IResourceFactory<out T, in TArg>
{
    /// <summary>
    /// Constructs resource from given argument.
    /// </summary>
    /// <param name="arg">Factory argument.</param>
    /// <returns>Constructed resource.</returns>
    T Load(TArg arg);
}