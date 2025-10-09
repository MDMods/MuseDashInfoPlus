using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MDIP.Application.DependencyInjection;

internal static class PropertyInjectionExtensions
{
    public static void AddSingletonWithPropertyInjection<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        services.AddSingleton(typeof(TService), provider =>
        {
            var instance = ActivatorUtilities.CreateInstance<TImplementation>(provider);
            return provider.InjectProperties(instance, true);
        });
    }

    internal static T InjectProperties<T>(this IServiceProvider provider, T instance, bool runPostInject = false)
    {
        if (instance == null)
            return default;

        foreach (var property in GetInjectableProperties(instance.GetType(), false))
        {
            var service = provider.GetService(property.PropertyType);
            if (service != null)
                property.SetValue(instance, service);
        }

        if (runPostInject && instance is IPostInjectable postInjectable)
            postInjectable.PostInject();

        return instance;
    }

    internal static void InjectStaticProperties(this IServiceProvider provider, Type type)
    {
        foreach (var property in GetInjectableProperties(type, true))
        {
            var service = provider.GetService(property.PropertyType);
            if (service != null)
                property.SetValue(null, service);
        }
    }

    private static IEnumerable<PropertyInfo> GetInjectableProperties(Type type, bool includeStatic)
    {
        var flags = BindingFlags.Public | BindingFlags.NonPublic | (includeStatic ? BindingFlags.Static : BindingFlags.Instance);

        return type
            .GetProperties(flags)
            .Where(property => property.SetMethod != null && HasUsedImplicitlyAttribute(property));
    }

    private static bool HasUsedImplicitlyAttribute(PropertyInfo property)
        => property.GetCustomAttributes(inherit: true).Select(attribute => attribute.GetType().FullName).Any(name => name is "JetBrains.Annotations.UsedImplicitlyAttribute" or "Il2CppJetBrains.Annotations.UsedImplicitlyAttribute");
}