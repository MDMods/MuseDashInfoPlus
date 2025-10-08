using System.Reflection;
using MDIP.Domain.Configs;

namespace MDIP.Infrastructure.Configuration;

public static class ConfigVersionControl
{
    public static T MigrateConfig<T>(T oldConfig, T newConfig) where T : ConfigBase
    {
        if (oldConfig == null || newConfig == null)
            return newConfig;

        var oldVersion = Version.Parse(oldConfig.Version);
        var newVersion = Version.Parse(newConfig.Version);

        if (oldVersion >= newVersion)
            return oldConfig;

        var migratedConfig = Activator.CreateInstance<T>();
        migratedConfig.Version = newConfig.Version;
        migratedConfig.LastModified = DateTime.Now;

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (property.Name is nameof(ConfigBase.Version) or nameof(ConfigBase.LastModified))
                continue;

            var oldValue = property.GetValue(oldConfig);
            var newValue = property.GetValue(newConfig);

            if (property.PropertyType.IsClass &&
                property.PropertyType != typeof(string) &&
                oldValue != null)
            {
                if (typeof(ConfigBase).IsAssignableFrom(property.PropertyType))
                {
                    var migrateMethod = typeof(ConfigVersionControl)
                        .GetMethod(nameof(MigrateConfig), BindingFlags.Public | BindingFlags.Static)
                        ?.MakeGenericMethod(property.PropertyType);
                    var migratedValue = migrateMethod?.Invoke(null, new[] { oldValue, newValue });
                    property.SetValue(migratedConfig, migratedValue);
                }
                else if (typeof(IEnumerable<object>).IsAssignableFrom(property.PropertyType))
                    property.SetValue(migratedConfig, oldValue);
                else
                {
                    var newObj = Activator.CreateInstance(property.PropertyType);
                    CopyProperties(oldValue, newObj);
                    property.SetValue(migratedConfig, newObj);
                }
            }
            else
                property.SetValue(migratedConfig, oldValue);
        }

        return migratedConfig;
    }

    private static void CopyProperties(object source, object target)
    {
        var properties = source.GetType().GetProperties();
        foreach (var prop in properties)
        {
            if (!prop.CanRead || !prop.CanWrite)
                continue;

            var value = prop.GetValue(source);
            prop.SetValue(target, value);
        }
    }
}