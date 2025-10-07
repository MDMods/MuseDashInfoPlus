namespace MDIP.Core.DependencyInjection;

public class ServiceCollection
{
    private readonly List<ServiceDescriptor> _descriptors = new();

    public ServiceCollection AddSingleton<TService, TImplementation>() where TService : class where TImplementation : class, TService
    {
        _descriptors.Add(ServiceDescriptor.Singleton(typeof(TService), typeof(TImplementation)));
        return this;
    }

    public ServiceCollection AddSingleton<TService>(Func<IServiceProvider, TService> factory) where TService : class
    {
        _descriptors.Add(ServiceDescriptor.Singleton(typeof(TService), provider => factory(provider)));
        return this;
    }

    public ServiceCollection AddSingleton<TService>(TService instance) where TService : class
    {
        _descriptors.Add(ServiceDescriptor.Singleton(typeof(TService), instance));
        return this;
    }

    public ServiceCollection AddSingleton<TService>() where TService : class, new()
    {
        _descriptors.Add(ServiceDescriptor.Singleton(typeof(TService), typeof(TService)));
        return this;
    }

    public ServiceCollection AddTransient<TService, TImplementation>() where TService : class where TImplementation : class, TService
    {
        _descriptors.Add(ServiceDescriptor.Transient(typeof(TService), typeof(TImplementation)));
        return this;
    }

    public ServiceCollection AddTransient<TService>(Func<IServiceProvider, TService> factory) where TService : class
    {
        _descriptors.Add(ServiceDescriptor.Transient(typeof(TService), provider => factory(provider)));
        return this;
    }

    public ServiceProvider BuildServiceProvider() => new ServiceProvider(_descriptors.ToArray());
}