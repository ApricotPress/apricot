namespace Apricot.Resources;

/// <summary>
/// Orchestrates <see cref="IResourceFactory{T,TArg}">resource factories</see> to load any asked resource.  
/// </summary>
public interface IResourcesLoader
{
    /// <summary>
    /// Tries to find factory for resources of type T that would load it out of provided argument.  
    /// </summary>
    /// <typeparam name="T">Type of resource we are trying to create.</typeparam>
    /// <typeparam name="TArg">Argument to create from.</typeparam>
    T Load<T, TArg>(TArg arg);

    
    /// <inheritdoc cref="Load{T,Targ}" />
    /// <remarks>
    /// Added here as a default type param which would be used with assets URIs.
    /// </remarks>
    T Load<T>(Uri arg) => Load<T, Uri>(arg);
}
