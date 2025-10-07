namespace MDIP.Core;

public static class ModServices
{
    private static IServiceProvider _provider;

    public static IServiceProvider Provider => _provider ?? throw new InvalidOperationException("Service provider not initialized.");

    public static void Initialize(IServiceProvider provider)
    {
        if (provider == null)
            throw new ArgumentNullException(nameof(provider));
        if (_provider != null)
            throw new InvalidOperationException("Services already initialized.");
        _provider = provider;
    }

    public static T GetRequiredService<T>() where T : notnull => Provider.GetRequiredService<T>();
    public static object GetRequiredService(Type serviceType) => Provider.GetRequiredService(serviceType);
}