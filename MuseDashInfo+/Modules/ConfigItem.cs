using MelonLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Version = System.Version;
using YamlDotNet.Serialization;

using MDIP.Modules.Configs;
using MDIP.Utils;
using MDIP.Attributes;

namespace MDIP.Modules
{
    public class ConfigItem
    {
        private readonly string _name;
        private readonly IDeserializer _deserializer;
        private readonly ISerializer _serializer;
        private readonly Dictionary<Type, object> _configCache;
        private readonly Dictionary<Type, Action<object>> _updateCallbacks;

        public string ConfigPath { get; }

        public ConfigItem(string name, string configPath, IDeserializer deserializer, ISerializer serializer)
        {
            _name = name;
            ConfigPath = configPath;
            _deserializer = deserializer;
            _serializer = serializer;
            _configCache = new Dictionary<Type, object>();
            _updateCallbacks = new Dictionary<Type, Action<object>>();
        }

        public T GetConfig<T>() where T : ConfigBase, new()
        {
            var type = typeof(T);

            if (_configCache.TryGetValue(type, out var cachedConfig))
                return (T)cachedConfig;

            var config = LoadConfig<T>();
            _configCache[type] = config;
            return config;
        }

        public void SaveConfig<T>(T config) where T : ConfigBase
        {
            config.LastModified = DateTime.Now;
            var comments = GetConfigComments<T>();
            var yaml = _serializer.Serialize(config);

            var lines = yaml.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            yaml = string.Empty;
            foreach (var line in lines)
            {
                yaml += line + Environment.NewLine;
                if (!line.StartsWith("version:") && !line.StartsWith("lastModified:"))
                    yaml += Environment.NewLine;
            }

            if (comments != null && comments.Count > 0)
                yaml = Utils.Configs.AddCommentsToYaml(yaml, comments);

            File.WriteAllText(ConfigPath, yaml);
            _configCache[typeof(T)] = config;
        }

        public void RegisterUpdateCallback<T>(Action<T> callback) where T : class
        {
            _updateCallbacks[typeof(T)] = obj => callback((T)obj);
        }

        public void ReloadConfig()
        {
            foreach (var type in _configCache.Keys)
            {
                var method = GetType().GetMethod(nameof(LoadConfig)).MakeGenericMethod(type);
                var newConfig = method.Invoke(this, null);
                _configCache[type] = newConfig;

                if (_updateCallbacks.TryGetValue(type, out var callback))
                {
                    callback(newConfig);
                }
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

        private T LoadConfig<T>() where T : ConfigBase, new()
        {
            if (!File.Exists(ConfigPath))
            {
                var defaultConfig = new T();
                SaveConfig(defaultConfig);
                return defaultConfig;
            }

            try
            {
                var yaml = File.ReadAllText(ConfigPath);
                var oldConfig = _deserializer.Deserialize<T>(yaml);
                var newConfig = new T();

                if (Version.Parse(oldConfig.Version) < Version.Parse(newConfig.Version))
                {
                    var backupPath = GetBackupPath(oldConfig.Version);
                    File.Copy(ConfigPath, backupPath, true);

                    var migratedConfig = VersionControl.MigrateConfig(oldConfig, newConfig);
                    SaveConfig(migratedConfig);
                    return migratedConfig;
                }

                return oldConfig;
            }
            catch (Exception ex)
            {
                Melon<MDIPMod>.Logger.BigError($"Failed to load config {_name}: {ex}");
                var defaultConfig = new T();
                SaveConfig(defaultConfig);
                return defaultConfig;
            }
        }

        private Dictionary<string, (string zh, string en)> GetConfigComments<T>() where T : ConfigBase
        {
            var comments = new Dictionary<string, (string zh, string en)>();
            var type = typeof(T);

            foreach (var property in type.GetProperties())
            {
                var commentZh = property.GetCustomAttribute<ConfigCommentZhAttribute>();
                var commentEn = property.GetCustomAttribute<ConfigCommentEnAttribute>();

                if ((commentZh == null || commentEn == null) && property.DeclaringType != null)
                {
                    var interfaces = property.DeclaringType.GetInterfaces();
                    foreach (var iface in interfaces)
                    {
                        var ifaceProp = iface.GetProperty(property.Name);
                        if (ifaceProp != null)
                        {
                            commentZh ??= ifaceProp.GetCustomAttribute<ConfigCommentZhAttribute>();
                            commentEn ??= ifaceProp.GetCustomAttribute<ConfigCommentEnAttribute>();
                        }
                    }
                }

                if (commentZh != null && commentEn != null)
                {
                    comments[$"{char.ToLowerInvariant(property.Name[0])}{property.Name[1..]}:"] =
                        (commentZh.Comment, commentEn.Comment);
                }
            }

            comments["version:"] = ("警告：不要改动下方的内容，否则配置或模组可能失效！", "Warning: Do not modify the content below, otherwise the configs or mod may become invalid!");

            return comments;
        }
    }
}