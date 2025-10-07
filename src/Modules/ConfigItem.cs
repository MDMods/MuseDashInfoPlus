﻿using System.Reflection;
using MDIP.Attributes;
using Version = System.Version;

namespace MDIP.Modules;

public class ConfigItem(
    string name,
    string configPath
)
{
    private Dictionary<Type, object> ConfigCache { get; } = new();
    private Dictionary<Type, Action<object>> UpdateCallbacks { get; } = new();

    public string ConfigPath { get; } = configPath;

    public T GetConfig<T>() where T : ConfigBase, new()
    {
        var type = typeof(T);

        if (ConfigCache.TryGetValue(type, out var cachedConfig))
            return (T)cachedConfig;

        var config = LoadConfig<T>();
        ConfigCache[type] = config;
        return config;
    }

    public void SaveConfig<T>(T config) where T : ConfigBase
    {
        config.LastModified = DateTime.Now;
        var comments = GetConfigComments<T>();
        var yaml = YamlParser.Serialize(config);

        var lines = yaml.Split(Environment.NewLine);
        yaml = string.Empty;
        foreach (var line in lines)
        {
            yaml += line + Environment.NewLine;
            if (!line.StartsWith("version:") && !line.StartsWith("lastModified:"))
                yaml += Environment.NewLine;
        }

        if (comments is { Count: > 0 })
            yaml = Utils.Configs.AddCommentsToYaml(yaml, comments);

        File.WriteAllText(ConfigPath, yaml);
        ConfigCache[typeof(T)] = config;
    }

    public void RegisterUpdateCallback<T>(Action<T> callback) where T : class
        => UpdateCallbacks[typeof(T)] = obj => callback((T)obj);

    public void ReloadConfig()
    {
        foreach (var type in ConfigCache.Keys)
        {
            var method = GetType().GetMethod(nameof(LoadConfig))?.MakeGenericMethod(type);
            var newConfig = method?.Invoke(this, null);
            ConfigCache[type] = newConfig;

            if (UpdateCallbacks.TryGetValue(type, out var callback)) callback(newConfig);
        }
    }

    private string GetBackupPath(string version)
    {
        var directory = Path.GetDirectoryName(ConfigPath) + "/Backups";
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        var fileName = Path.GetFileNameWithoutExtension(ConfigPath);
        var extension = Path.GetExtension(ConfigPath);
        return Path.Combine(directory, $"{fileName}.v{version}{extension}");
    }

    public T LoadConfig<T>() where T : ConfigBase, new()
    {
        if (!File.Exists(ConfigPath))
            return CreateAndSaveDefault<T>();

        try
        {
            var oldConfig = YamlParser.Deserialize<T>(File.ReadAllText(ConfigPath));
            var newConfig = new T();

            var oldVersion = Version.Parse(oldConfig.Version);
            var newVersion = Version.Parse(newConfig.Version);
            if (oldVersion >= newVersion)
                return oldConfig;

            File.Copy(ConfigPath, GetBackupPath(oldConfig.Version), true);

            // Config files for versions prior to 2.3.0 are no longer compatible
            var oldConfigIncompatible = oldVersion < Version.Parse("2.3.0")
                                        && name is not "MainConfigs" and not "AdvancedConfigs";
            if (oldConfigIncompatible)
                Melon<MDIPMod>.Logger.Warning($"{name} will be restored to default because the configs before 2.3.0 are no longer compatible");
            var finalConfig = oldConfigIncompatible
                ? newConfig
                : VersionControl.MigrateConfig(oldConfig, newConfig);

            SaveConfig(finalConfig);
            return finalConfig;
        }
        catch (Exception ex)
        {
            Melon<MDIPMod>.Logger.BigError($"Failed to load config {name}: {ex}");
            return CreateAndSaveDefault<T>();
        }
    }

    private T CreateAndSaveDefault<T>() where T : ConfigBase, new()
    {
        var defaultConfig = new T();
        SaveConfig(defaultConfig);
        return defaultConfig;
    }

    private static Dictionary<string, (string zh, string en)> GetConfigComments<T>() where T : ConfigBase
    {
        var comments = typeof(T).GetProperties()
            .Select(prop => (
                Property: prop,
                Comments: GetComments(prop)
            ))
            .Where(x => x.Comments is { zh: not null, en: not null })
            .ToDictionary(
                x => ToCamelCase(x.Property.Name) + ":",
                x => (x.Comments.zh!.Comment, x.Comments.en!.Comment)
            );

        comments["version:"] = ("警告：不要改动下方的内容，否则配置或模组可能失效！",
            "Warning: Do not modify the content below, otherwise the configs or mod may become invalid!");

        return comments;
    }

    private static (ConfigCommentZhAttribute zh, ConfigCommentEnAttribute en) GetComments(PropertyInfo property)
    {
        var zh = property.GetCustomAttribute<ConfigCommentZhAttribute>();
        var en = property.GetCustomAttribute<ConfigCommentEnAttribute>();

        if (zh != null && en != null || property.DeclaringType == null)
            return (zh, en);

        foreach (var iface in property.DeclaringType.GetInterfaces())
        {
            var ifaceProp = iface.GetProperty(property.Name);
            if (ifaceProp == null) continue;

            zh ??= ifaceProp.GetCustomAttribute<ConfigCommentZhAttribute>();
            en ??= ifaceProp.GetCustomAttribute<ConfigCommentEnAttribute>();
        }

        return (zh, en);
    }

    private static string ToCamelCase(string str) =>
        char.ToLowerInvariant(str[0]) + str[1..];
}