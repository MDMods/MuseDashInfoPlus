namespace MDIP.Core.DependencyInjection;

public class ServiceProvider : IServiceProvider
{
    private readonly Dictionary<Type, ServiceDescriptor> _descriptors;
    private readonly Dictionary<Type, object> _singletons = new();

    public ServiceProvider(IEnumerable<ServiceDescriptor> descriptors)
    {
        _descriptors = new();
        foreach (var descriptor in descriptors)
            _descriptors[descriptor.ServiceType] = descriptor;
    }

    public object GetService(Type serviceType)
    {
        if (serviceType == typeof(IServiceProvider))
            return this;

        if (!_descriptors.TryGetValue(serviceType, out var descriptor))
            return null;

        if (descriptor.ImplementationInstance != null)
            return descriptor.ImplementationInstance;

        if (descriptor.Lifetime == ServiceLifetime.Singleton && _singletons.TryGetValue(serviceType, out var instance))
            return instance;

        var implementation = descriptor.ImplementationFactory != null
            ? descriptor.ImplementationFactory(this)
            : CreateInstance(descriptor.ImplementationType);

        if (implementation == null)
            return null;

        if (descriptor.Lifetime == ServiceLifetime.Singleton)
            _singletons[serviceType] = implementation;

        return implementation;
    }

    private object CreateInstance(Type implementationType)
    {
        if (implementationType == null)
            return null;

        var constructors = implementationType.GetConstructors();
        var constructor = constructors.OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();
        if (constructor == null)
            return Activator.CreateInstance(implementationType);

        var parameters = constructor.GetParameters();
        if (parameters.Length == 0)
            return Activator.CreateInstance(implementationType);

        var arguments = new object[parameters.Length];
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameterInstance = GetService(parameters[i].ParameterType);
            if (parameterInstance == null)
                throw new InvalidOperationException($"Service of type {parameters[i].ParameterType.FullName} is not registered.");
            arguments[i] = parameterInstance;
        }

        return Activator.CreateInstance(implementationType, arguments);
    }
}