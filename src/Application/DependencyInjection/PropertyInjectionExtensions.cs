using System.Reflection;

namespace MDIP.Application.DependencyInjection;

internal static class PropertyInjectionExtensions
{
    public static void AddSingletonWithPropertyInjection<TService, TImplementation>(this SimpleServiceProvider provider)
        where TService : class
        where TImplementation : class, TService
        => provider.AddSingleton(typeof(TService), typeof(TImplementation), true);

    public static void AddScopedWithPropertyInjection<TService, TImplementation>(this SimpleServiceProvider provider)
        where TService : class
        where TImplementation : class, TService
        => provider.AddScoped(typeof(TService), typeof(TImplementation), true);

    public static T InjectProperties<T>(this SimpleServiceProvider provider, T instance)
    {
        if (instance == null)
            return default;

        foreach (var property in GetInjectableProperties(instance.GetType(), includeStatic: false))
        {
            var service = provider.GetService(property.PropertyType);
            if (service != null)
                property.SetValue(instance, service);
        }

        return instance;
    }

    public static void InjectStaticProperties(this SimpleServiceProvider provider, Type type)
    {
        foreach (var property in GetInjectableProperties(type, includeStatic: true))
        {
            var service = provider.GetService(property.PropertyType);
            if (service != null)
                property.SetValue(null, service);
        }
    }

    private static IEnumerable<PropertyInfo> GetInjectableProperties(Type type, bool includeStatic)
    {
        const BindingFlags sharedFlags = BindingFlags.Public | BindingFlags.NonPublic;
        var flags = includeStatic ? sharedFlags | BindingFlags.Static : sharedFlags | BindingFlags.Instance;

        return type
            .GetProperties(flags)
            .Where(property => IsInjectable(property, includeStatic));
    }

    private static bool IsInjectable(PropertyInfo property, bool includeStatic)
    {
        var setMethod = property.SetMethod;
        if (setMethod == null)
            return false;

        if (setMethod.IsStatic != includeStatic)
            return false;

        if (property.GetIndexParameters().Length != 0)
            return false;

        return property.PropertyType.IsInterface || property.PropertyType.IsAbstract ||
               HasUsedImplicitlyAttribute(property);
    }

    private static bool HasUsedImplicitlyAttribute(PropertyInfo property)
        => property.GetCustomAttributes(inherit: true)
            .Select(attribute => attribute.GetType().FullName)
            .Any(name => name is
                "JetBrains.Annotations.UsedImplicitlyAttribute" or
                "Il2CppJetBrains.Annotations.UsedImplicitlyAttribute");
}