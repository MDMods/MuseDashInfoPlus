namespace MDIP.Core.DependencyInjection;

public static class ServiceProviderExtensions
{
    public static T GetRequiredService<T>(this IServiceProvider provider) where T : notnull
    {
        var service = provider.GetService(typeof(T));
        if (service is T typed)
            return typed;
        throw new InvalidOperationException($"Service of type {typeof(T).FullName} is not registered.");
    }

    public static object GetRequiredService(this IServiceProvider provider, Type serviceType)
    {
        var service = provider.GetService(serviceType);
        if (service != null)
            return service;
        throw new InvalidOperationException($"Service of type {serviceType.FullName} is not registered.");
    }
}