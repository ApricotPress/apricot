using Microsoft.Extensions.DependencyInjection;

namespace Apricot.Resources;

/// <inheritdoc />
public class Resources(
    IServiceProvider services
) : IResources
{
    /// <inheritdoc />
    public T Load<T, TArg>(TArg arg)
    {
        var factory = services.GetService<IResourceFactory<T, TArg>>();

        if (factory is null)
        {
            throw new NotSupportedException($"No factory was found to construct {typeof(T)} from {typeof(TArg)}");
        }

        return factory.Load(arg);
    }
}
