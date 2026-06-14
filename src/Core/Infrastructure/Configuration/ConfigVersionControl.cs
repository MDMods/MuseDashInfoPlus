using System.Reflection;
using MDIP.Core.Domain.Configs;

namespace MDIP.Core.Infrastructure.Configuration;

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

        // The config schema is flat (string/int/float/bool/DateTime properties only), so migration
        // is a straight copy of each carried-over value into a fresh instance stamped at the new
        // version. Nested-object / collection deep-migration was dead code (no config has such a
        // property) and was removed.
        foreach (var property in properties)
        {
            if (property.Name is nameof(ConfigBase.Version) or nameof(ConfigBase.LastModified))
                continue;

            property.SetValue(migratedConfig, property.GetValue(oldConfig));
        }

        return migratedConfig;
    }
}