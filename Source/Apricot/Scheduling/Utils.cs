using Microsoft.Extensions.DependencyInjection;

namespace Apricot.Scheduling;

public static class Utils
{
    public static void CastCallback<T>(this IServiceProvider services, Action<T> callback)
    {
        using var scope = services.CreateScope();
        
        foreach (var service in scope.ServiceProvider.GetServices<T>())
        {
            callback(service);
        }
    }
}
