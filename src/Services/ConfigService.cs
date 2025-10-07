namespace MDIP.Services;

public class ConfigService : IConfigService
{
    private readonly MelonLogger.Instance _logger;
    private readonly Dictionary<string, ConfigItem> _modules = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _syncRoot = new();
    private readonly Dictionary<string, FileSystemWatcher> _watchers = new(StringComparer.OrdinalIgnoreCase);
    private string _configDirectory;
    private bool _watchersActivated;

    public ConfigService(MelonLogger.Instance logger) => _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public void Init()
    {
        lock (_syncRoot)
        {
            foreach (var watcher in _watchers.Values)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Changed -= OnConfigFileChanged;
                watcher.Created -= OnConfigFileChanged;
                watcher.Renamed -= OnConfigFileRenamed;
                watcher.Dispose();
            }
            _watchers.Clear();
            _modules.Clear();
            _watchersActivated = false;
        }
    }

    public void ActivateWatcher()
    {
        lock (_syncRoot)
        {
            foreach (var watcher in _watchers.Values)
                watcher.EnableRaisingEvents = true;
            _watchersActivated = true;
        }
    }

    public void RegisterModule(string name, string configFileName)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Module name cannot be null or empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(configFileName))
            throw new ArgumentException("Config file name cannot be null or empty.", nameof(configFileName));

        lock (_syncRoot)
        {
            if (_modules.ContainsKey(name))
            {
                _logger.Error($"Module {name} already exists.");
                return;
            }

            var configPath = GetConfigFilePath(configFileName);
            var directory = Path.GetDirectoryName(configPath);
            if (string.IsNullOrWhiteSpace(directory))
                throw new DirectoryNotFoundException($"Configs directory {configFileName} not found.");

            if (!_watchers.ContainsKey(directory))
            {
                var watcher = CreateWatcher(directory);
                _watchers.Add(directory, watcher);
                if (_watchersActivated)
                    watcher.EnableRaisingEvents = true;
            }

            var module = new ConfigItem(name, configPath);
            _modules.Add(name, module);
        }
    }

    public T GetConfig<T>(string moduleName) where T : ConfigBase, new()
    {
        lock (_syncRoot)
        {
            if (!_modules.TryGetValue(moduleName, out var module))
                throw new InvalidOperationException($"Module {moduleName} not found.");
            return module.GetConfig<T>();
        }
    }

    public void SaveConfig<T>(string moduleName, T config) where T : ConfigBase
    {
        lock (_syncRoot)
        {
            if (!_modules.TryGetValue(moduleName, out var module))
                throw new InvalidOperationException($"Module {moduleName} not found.");
            module.SaveConfig(config);
        }
    }

    public void RegisterUpdateCallback<T>(string moduleName, Action<T> callback) where T : class
    {
        if (callback == null)
            return;

        lock (_syncRoot)
        {
            if (!_modules.TryGetValue(moduleName, out var module))
                throw new InvalidOperationException($"Module {moduleName} not found.");
            module.RegisterUpdateCallback(callback);
        }
    }

    public string GetConfigFilePath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));

        var directory = EnsureConfigDirectory();
        var fullPath = Path.GetFullPath(Path.Combine(directory, fileName));
        return fullPath;
    }

    private string EnsureConfigDirectory()
    {
        if (!string.IsNullOrEmpty(_configDirectory))
            return _configDirectory;

        var directory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "UserData", "Info+"));
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            _logger.Msg("Created config directory.");
        }

        _configDirectory = directory;
        return directory;
    }

    private FileSystemWatcher CreateWatcher(string directory)
    {
        var watcher = new FileSystemWatcher(directory, "*.yml")
        {
            NotifyFilter = NotifyFilters.Attributes
                           | NotifyFilters.CreationTime
                           | NotifyFilters.DirectoryName
                           | NotifyFilters.FileName
                           | NotifyFilters.LastAccess
                           | NotifyFilters.LastWrite
                           | NotifyFilters.Security
                           | NotifyFilters.Size,
            IncludeSubdirectories = false,
            EnableRaisingEvents = false
        };

        watcher.Changed += OnConfigFileChanged;
        watcher.Created += OnConfigFileChanged;
        watcher.Renamed += OnConfigFileRenamed;

        return watcher;
    }

    private void OnConfigFileRenamed(object sender, RenamedEventArgs e)
    {
        OnConfigFileChanged(sender, e);
    }

    private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        ConfigItem module;

        lock (_syncRoot)
        {
            module = _modules.Values.FirstOrDefault(m => string.Equals(m.ConfigPath, e.FullPath, StringComparison.OrdinalIgnoreCase));
        }

        if (module == null)
            return;

        _logger.Msg($"Config file changed: {e.Name}");
        for (var i = 0; i < 3; i++)
        {
            try
            {
                Thread.Sleep(100);
                module.ReloadConfig();
                _logger.Msg($"Reloaded: {e.Name}");
                return;
            }
            catch (IOException)
            {
            }
        }
    }
}