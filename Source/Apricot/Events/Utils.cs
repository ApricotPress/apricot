using Microsoft.Extensions.DependencyInjection;

namespace Apricot.Events;

public static class Utils
{
    /// <summary>
    /// Creates scope from services provider and calls <see cref="callback"/> for each found <see cref="T"/> in
    /// container. Mainly used for event listeners.
    /// </summary>
    /// <param name="services">IoC</param>
    /// <param name="callback">Callback to call for each service.</param>
    /// <typeparam name="T">Type of services to look for.</typeparam>
    public static void CastCallback<T>(this IServiceProvider services, Action<T> callback)
    {
        using var scope = services.CreateScope();

        foreach (var service in scope.ServiceProvider.GetServices<T>())
        {
            callback(service);
        }
    }
}
