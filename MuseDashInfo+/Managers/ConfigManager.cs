namespace MDIP.Managers;

public static class ConfigManager
{
	private static readonly Dictionary<string, ConfigItem> _modules = new();
	private static FileSystemWatcher _watcher = new();

	public static void Init()
	{
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

	public static void ActivateWatcher() => _watcher.EnableRaisingEvents = true;

	public static void RegisterModule(string name, string configFileName)
	{
		if (_modules.ContainsKey(name))
		{
			Melon<MDIPMod>.Logger.Error($"Module {name} already exists");
			return;
		}

		var configPath = Configs.GetConfigPath(configFileName);
		var module = new ConfigItem(name, configPath);
		_modules[name] = module;

		var path = Path.GetDirectoryName(configPath);
		if (string.IsNullOrWhiteSpace(path))
			throw new DirectoryNotFoundException($"Configs directory {configFileName} not found");

		_watcher.Path = path;
	}

	public static T GetConfig<T>(string moduleName) where T : ConfigBase, new()
	{
		if (!_modules.TryGetValue(moduleName, out var module))
			throw new($"Module {moduleName} not found");

		return module.GetConfig<T>();
	}

	public static void SaveConfig<T>(string moduleName, T config) where T : ConfigBase
	{
		if (!_modules.TryGetValue(moduleName, out var module))
			throw new($"Module {moduleName} not found");

		module.SaveConfig(config);
	}

	private static void OnConfigFileChanged(object sender, FileSystemEventArgs e)
	{
		Melon<MDIPMod>.Logger.Msg($"Config file changed: {e.Name}");
		for (var i = 0; i < 3; i++)
		{
			try
			{
				Thread.Sleep(100);
				foreach (var module in _modules.Values.Where(module => module.ConfigPath == e.FullPath))
				{
					Melon<MDIPMod>.Logger.Msg($"Reloading: {e.Name}");
					module.ReloadConfig();
					break;
				}

				return;
			}
			catch (IOException)
			{ }
		}
	}
}