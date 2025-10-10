using System.Reflection;

namespace MDIP.Application.DependencyInjection;

/// <summary>
/// Scoped resolves only inside an active scope; outside returns null.
/// Singletons with property injection are re-injected after each CreateScope().
/// Ctor: pick the public one with all-registrable params; else fail.
/// Circular deps detected via _resolving.
/// </summary>
public sealed class SimpleServiceProvider : IServiceProvider
{
    public enum ServiceLifetime
    {
        Singleton,
        Scoped
    }

    private readonly Dictionary<Type, ServiceDescriptor> _serviceDescriptors = new();
    private readonly Dictionary<Type, ServiceDescriptor> _implementationDescriptors = new();
    private readonly List<ServiceDescriptor> _openGenericDescriptors = [];
    private readonly Dictionary<Type, object> _singletons = new();
    private readonly Dictionary<Type, object> _scopedInstances = new();
    private readonly HashSet<Type> _resolving = [];
    private readonly List<object> _propertyInjectedSingletons = [];
    private int _scopeDepth;

    public object GetService(Type serviceType)
    {
        var descriptor = ResolveDescriptor(serviceType);
        if (descriptor == null)
            return null;

        return descriptor.Lifetime switch
        {
            ServiceLifetime.Singleton => GetOrCreateSingleton(serviceType, descriptor),
            ServiceLifetime.Scoped => GetOrCreateScoped(serviceType, descriptor),
            _ => throw new InvalidOperationException($"Unknown service lifetime: {descriptor.Lifetime}")
        };
    }

    public void AddSingleton(Type serviceType, Type implementationType, bool usePropertyInjection = false)
        => AddService(serviceType, implementationType, ServiceLifetime.Singleton, usePropertyInjection);

    public void AddScoped(Type serviceType, Type implementationType, bool usePropertyInjection = false)
        => AddService(serviceType, implementationType, ServiceLifetime.Scoped, usePropertyInjection);

    private void AddService(Type serviceType, Type implementationType, ServiceLifetime lifetime, bool usePropertyInjection)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        ArgumentNullException.ThrowIfNull(implementationType);
        if (implementationType.IsAbstract || implementationType.IsInterface)
            throw new InvalidOperationException($"Implementation type {implementationType} must be a non-abstract class.");

        var descriptor = new ServiceDescriptor(serviceType, implementationType, lifetime, usePropertyInjection);

        if (serviceType.IsGenericTypeDefinition)
            _openGenericDescriptors.Add(descriptor);
        else
            _serviceDescriptors[serviceType] = descriptor;

        if (!implementationType.IsGenericTypeDefinition)
            _implementationDescriptors[implementationType] = descriptor;
        else if (!serviceType.IsGenericTypeDefinition)
            throw new InvalidOperationException("Generic implementation type requires a matching generic service type.");
    }

    public IServiceScope CreateScope() => new ServiceScope(this);

    public void ClearScope()
    {
        foreach (var instance in _scopedInstances.Values)
        {
            if (instance is IDisposable disposable)
                disposable.Dispose();
        }
        _scopedInstances.Clear();
    }

    public void RefreshSingletonPropertyInjections()
    {
        foreach (var instance in _propertyInjectedSingletons)
            this.InjectProperties(instance);
    }

    public T GetRequiredService<T>() where T : class => (T)GetRequiredService(typeof(T));

    public object GetRequiredService(Type serviceType)
        => GetService(serviceType) ?? throw new InvalidOperationException($"Service of type {serviceType} is not registered.");

    private object GetOrCreateSingleton(Type serviceType, ServiceDescriptor descriptor)
    {
        if (_singletons.TryGetValue(serviceType, out var existing))
            return existing;

        var instance = CreateInstance(serviceType, descriptor);
        _singletons[serviceType] = instance;

        if (serviceType != descriptor.ImplementationType)
            _singletons.TryAdd(descriptor.ImplementationType, instance);

        if (descriptor.UsePropertyInjection)
            _propertyInjectedSingletons.Add(instance);

        return instance;
    }

    private object GetOrCreateScoped(Type serviceType, ServiceDescriptor descriptor)
    {
        if (_scopeDepth <= 0)
            return null;

        if (_scopedInstances.TryGetValue(serviceType, out var existing))
            return existing;

        var instance = CreateInstance(serviceType, descriptor);
        _scopedInstances[serviceType] = instance;

        if (serviceType != descriptor.ImplementationType)
            _scopedInstances.TryAdd(descriptor.ImplementationType, instance);

        return instance;
    }

    private object CreateInstance(Type serviceType, ServiceDescriptor descriptor)
    {
        var implementationType = descriptor.ImplementationType;

        if (implementationType.IsGenericTypeDefinition)
        {
            if (!serviceType.IsGenericType)
                throw new InvalidOperationException($"Service type {serviceType} must be generic to use implementation {implementationType}.");
            implementationType = implementationType.MakeGenericType(serviceType.GetGenericArguments());
        }

        if (!_resolving.Add(implementationType))
            throw new InvalidOperationException($"Circular dependency detected while resolving {implementationType}.");

        try
        {
            var instance = CreateInstanceCore(implementationType);

            if (descriptor.UsePropertyInjection)
                this.InjectProperties(instance);

            return instance;
        }
        finally
        {
            _resolving.Remove(implementationType);
        }
    }

    private ServiceDescriptor ResolveDescriptor(Type serviceType)
    {
        if (_serviceDescriptors.TryGetValue(serviceType, out var descriptor) ||
            _implementationDescriptors.TryGetValue(serviceType, out descriptor))
            return descriptor;

        if (!serviceType.IsGenericType)
            return _serviceDescriptors.Values.FirstOrDefault(entry => serviceType.IsAssignableFrom(entry.ImplementationType));

        var definition = serviceType.GetGenericTypeDefinition();
        descriptor = _openGenericDescriptors.FirstOrDefault(d => d.ServiceType == definition);
        if (descriptor != null)
            return descriptor;

        descriptor = _openGenericDescriptors.FirstOrDefault(d => d.ImplementationType == definition);
        return descriptor ?? _serviceDescriptors.Values.FirstOrDefault(entry => serviceType.IsAssignableFrom(entry.ImplementationType));
    }

    private object CreateInstanceCore(Type implementationType)
    {
        var constructors = implementationType
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .OrderByDescending(ctor => ctor.GetParameters().Length)
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

    private sealed class ServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime, bool usePropertyInjection)
    {
        public Type ServiceType { get; } = serviceType;
        public Type ImplementationType { get; } = implementationType;
        public ServiceLifetime Lifetime { get; } = lifetime;
        public bool UsePropertyInjection { get; } = usePropertyInjection;
    }

    private sealed class ServiceScope : IServiceScope
    {
        private readonly SimpleServiceProvider _provider;
        private bool _disposed;

        public ServiceScope(SimpleServiceProvider provider)
        {
            _provider = provider;
            Interlocked.Increment(ref _provider._scopeDepth);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _provider.ClearScope();
            Interlocked.Decrement(ref _provider._scopeDepth);
        }
    }
}

public interface IServiceScope : IDisposable
{
}