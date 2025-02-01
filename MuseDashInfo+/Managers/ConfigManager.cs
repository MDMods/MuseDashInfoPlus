using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using MDIP.Modules;
using MDIP.Modules.Configs;
using MDIP.Utils;
using MelonLoader;

namespace MDIP.Managers;

public class ConfigManager : IDisposable
{
	private static readonly object _lock = new();
	private static ConfigManager _instance;

	private readonly Dictionary<string, ConfigItem> _modules;
	private readonly FileSystemWatcher _watcher;
	private readonly YamlParser _yamlParser;
	private bool _disposed;

	public static ConfigManager Instance
	{
		get
		{
			if (_instance == null)
			{
				lock (_lock)
				{
					_instance ??= new();
				}
			}

			return _instance;
		}
	}

	private ConfigManager()
	{
		_modules = new();
		_yamlParser = new();

		_watcher = new()
		{
			Filter = "*.yml",
			NotifyFilter = NotifyFilters.Attributes
			               | NotifyFilters.CreationTime
			               | NotifyFilters.DirectoryName
			               | NotifyFilters.FileName
			               | NotifyFilters.LastAccess
			               | NotifyFilters.LastWrite
			               | NotifyFilters.Security
			               | NotifyFilters.Size
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

	public void ActivateWatcher() => _watcher.EnableRaisingEvents = true;

	public void RegisterModule(string name, string configFileName)
	{
		if (_modules.ContainsKey(name))
		{
			Melon<MDIPMod>.Logger.Error($"Module {name} already exists");
			return;
		}

		var configPath = Configs.GetConfigPath(configFileName);
		var module = new ConfigItem(name, configPath, _yamlParser);
		_modules[name] = module;

		_watcher.Path = Path.GetDirectoryName(configPath);
	}

	public ConfigItem GetModule(string moduleName)
	{
		if (!_modules.TryGetValue(moduleName, out var module))
			throw new($"Module {moduleName} not found");

		return module;
	}

	public T GetConfig<T>(string moduleName) where T : ConfigBase, new()
	{
		if (!_modules.TryGetValue(moduleName, out var module))
			throw new($"Module {moduleName} not found");

		return module.GetConfig<T>();
	}

	public void SaveConfig<T>(string moduleName, T config) where T : ConfigBase
	{
		if (!_modules.TryGetValue(moduleName, out var module))
			throw new($"Module {moduleName} not found");

		module.SaveConfig(config);
	}

	private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
	{
		Melon<MDIPMod>.Logger.Msg($"Config file changed: {e.Name}");
		for (var i = 0; i < 3; i++)
		{
			try
			{
				Thread.Sleep(100);
				foreach (var module in _modules.Values)
				{
					if (module.ConfigPath == e.FullPath)
					{
						Melon<MDIPMod>.Logger.Msg($"Reloading: {e.Name}");
						module.ReloadConfig();
						break;
					}
				}

				return;
			}
			catch (IOException)
			{ }
		}
	}
}