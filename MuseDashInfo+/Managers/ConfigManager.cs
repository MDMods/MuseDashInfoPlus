using MelonLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

using MDIP.Modules;
using MDIP.Modules.Configs;
using MDIP.Utils;

namespace MDIP.Managers
{
    public class ConfigManager : IDisposable
    {
        private static readonly object _lock = new();
        private static ConfigManager _instance;
        public static ConfigManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new ConfigManager();
                    }
                }
                return _instance;
            }
        }

        private readonly Dictionary<string, ConfigItem> _modules;
        private readonly FileSystemWatcher _watcher;
        private readonly IDeserializer _deserializer;
        private readonly ISerializer _serializer;
        private bool _disposed;

        private ConfigManager()
        {
            _modules = new Dictionary<string, ConfigItem>();
            _deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithAttemptingUnquotedStringTypeDeserialization()
                .Build();
            _serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            _watcher = new FileSystemWatcher
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime
            };

            _watcher.Changed += OnConfigFileChanged;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing) _watcher?.Dispose();

            _disposed = true;
        }

        public void RegisterModule(string name, string configFileName)
        {
            if (_modules.ContainsKey(name))
            {
                Melon<MDIPMod>.Logger.Error($"Module {name} already exists");
                return;
            }

            var configPath = Configs.GetConfigPath(configFileName);
            var module = new ConfigItem(name, configPath, _deserializer, _serializer);
            _modules[name] = module;

            _watcher.Path = Path.GetDirectoryName(configPath);
            _watcher.Filter = Path.GetFileName(configPath);
            _watcher.EnableRaisingEvents = true;
        }

        public ConfigItem GetModule(string moduleName)
        {
            if (!_modules.TryGetValue(moduleName, out var module))
                throw new Exception($"Module {moduleName} not found");

            return module;
        }

        public T GetConfig<T>(string moduleName) where T : ConfigBase, new()
        {
            if (!_modules.TryGetValue(moduleName, out var module))
                throw new Exception($"Module {moduleName} not found");

            return module.GetConfig<T>();
        }

        public void SaveConfig<T>(string moduleName, T config) where T : ConfigBase
        {
            if (!_modules.TryGetValue(moduleName, out var module))
                throw new Exception($"Module {moduleName} not found");

            module.SaveConfig(config);
        }

        private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
        {
            foreach (var module in _modules.Values)
            {
                if (module.ConfigPath == e.FullPath)
                {
                    Thread.Sleep(100); // Wait for file to be released
                    module.ReloadConfig();
                    break;
                }
            }
        }
    }
}