using System.Reflection;

namespace MDIP.Application.DependencyInjection;

public sealed class SimpleServiceProvider : IServiceProvider
{
    private sealed class ServiceDescriptor(Type serviceType, Type implementationType, bool usePropertyInjection)
    {
        public Type ServiceType { get; } = serviceType;
        public Type ImplementationType { get; } = implementationType;
        public bool UsePropertyInjection { get; } = usePropertyInjection;
    }

    private readonly Dictionary<Type, ServiceDescriptor> _serviceDescriptors = new();
    private readonly Dictionary<Type, ServiceDescriptor> _implementationDescriptors = new();
    private readonly Dictionary<Type, object> _singletons = new();
    private readonly List<ServiceDescriptor> _openGenericDescriptors = [];
    private readonly HashSet<Type> _resolving = [];

    public void AddSingleton(Type serviceType, Type implementationType, bool usePropertyInjection = false)
    {
        if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
        if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
        if (implementationType.IsAbstract || implementationType.IsInterface)
            throw new InvalidOperationException($"Implementation type {implementationType} must be a non-abstract class.");

        var descriptor = new ServiceDescriptor(serviceType, implementationType, usePropertyInjection);

        if (serviceType.IsGenericTypeDefinition)
            _openGenericDescriptors.Add(descriptor);
        else
            _serviceDescriptors[serviceType] = descriptor;

        if (!implementationType.IsGenericTypeDefinition)
            _implementationDescriptors[implementationType] = descriptor;
        else if (!serviceType.IsGenericTypeDefinition)
            throw new InvalidOperationException("Generic implementation type requires a matching generic service type.");
    }

    public T GetRequiredService<T>() where T : class => (T)GetRequiredService(typeof(T));

    public object GetRequiredService(Type serviceType)
        => GetService(serviceType) ?? throw new InvalidOperationException($"Service of type {serviceType} is not registered.");

    public object GetService(Type serviceType)
    {
        if (_singletons.TryGetValue(serviceType, out var existing))
            return existing;

        var descriptor = ResolveDescriptor(serviceType);
        if (descriptor == null)
            return null;

        if (!_resolving.Add(serviceType))
            throw new InvalidOperationException($"Circular dependency detected while resolving {serviceType}.");

        try
        {
            var implementationType = descriptor.ImplementationType;
            if (implementationType.IsGenericTypeDefinition)
            {
                if (!serviceType.IsGenericType)
                    throw new InvalidOperationException($"Service type {serviceType} must be generic to use implementation {implementationType}.");
                implementationType = implementationType.MakeGenericType(serviceType.GetGenericArguments());
            }

            var instance = CreateInstance(implementationType);

            if (descriptor.UsePropertyInjection)
                this.InjectProperties(instance);

            _singletons[serviceType] = instance;

            if (serviceType != implementationType)
                _singletons.TryAdd(implementationType, instance);

            return instance;
        }
        finally
        {
            _resolving.Remove(serviceType);
        }
    }

    private ServiceDescriptor ResolveDescriptor(Type serviceType)
    {
        if (_serviceDescriptors.TryGetValue(serviceType, out var descriptor))
            return descriptor;

        if (_implementationDescriptors.TryGetValue(serviceType, out descriptor))
            return descriptor;

        if (serviceType.IsGenericType)
        {
            var definition = serviceType.GetGenericTypeDefinition();
            descriptor = _openGenericDescriptors.FirstOrDefault(d => d.ServiceType == definition);
            if (descriptor != null)
                return descriptor;

            descriptor = _openGenericDescriptors.FirstOrDefault(d => d.ImplementationType == definition);
            if (descriptor != null)
                return descriptor;
        }

        foreach (var entry in _serviceDescriptors.Values)
        {
            if (serviceType.IsAssignableFrom(entry.ImplementationType))
                return entry;
        }

        return null;
    }

    private object CreateInstance(Type implementationType)
    {
        var constructors = implementationType
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .OrderBy(ctor => ctor.GetParameters().Length)
            .ToArray();

        if (constructors.Length == 0)
            throw new InvalidOperationException($"No public constructor found for {implementationType}.");

        foreach (var ctor in constructors)
        {
            var parameters = ctor.GetParameters();
            if (parameters.Length == 0)
                return Activator.CreateInstance(implementationType);

            var args = new object[parameters.Length];
            var canUse = true;

            for (var i = 0; i < parameters.Length; i++)
            {
                var service = GetService(parameters[i].ParameterType);
                if (service == null)
                {
                    canUse = false;
                    break;
                }

                args[i] = service;
            }

            if (canUse)
                return ctor.Invoke(args);
        }

        throw new InvalidOperationException($"Unable to resolve constructor for {implementationType}.");
    }
}