namespace MDIP.Core.DependencyInjection;

public sealed class ServiceDescriptor
{
    public Type ServiceType { get; }
    public Type ImplementationType { get; }
    public Func<IServiceProvider, object> ImplementationFactory { get; }
    public object ImplementationInstance { get; }
    public ServiceLifetime Lifetime { get; }

    private ServiceDescriptor(Type serviceType, ServiceLifetime lifetime)
    {
        ServiceType = serviceType;
        Lifetime = lifetime;
    }

    public ServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime) : this(serviceType, lifetime) => ImplementationType = implementationType;

    public ServiceDescriptor(Type serviceType, Func<IServiceProvider, object> implementationFactory, ServiceLifetime lifetime) : this(serviceType, lifetime) => ImplementationFactory = implementationFactory;

    public ServiceDescriptor(Type serviceType, object implementationInstance) : this(serviceType, ServiceLifetime.Singleton) => ImplementationInstance = implementationInstance;

    public static ServiceDescriptor Singleton(Type serviceType, Type implementationType) => new(serviceType, implementationType, ServiceLifetime.Singleton);
    public static ServiceDescriptor Singleton(Type serviceType, Func<IServiceProvider, object> implementationFactory) => new(serviceType, implementationFactory, ServiceLifetime.Singleton);
    public static ServiceDescriptor Singleton(Type serviceType, object implementationInstance) => new(serviceType, implementationInstance);
    public static ServiceDescriptor Transient(Type serviceType, Type implementationType) => new(serviceType, implementationType, ServiceLifetime.Transient);
    public static ServiceDescriptor Transient(Type serviceType, Func<IServiceProvider, object> implementationFactory) => new(serviceType, implementationFactory, ServiceLifetime.Transient);
}