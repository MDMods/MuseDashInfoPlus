using JetBrains.Annotations;
using MDIP.Application.DependencyInjection;
using MDIP.Application.Services.Global.Logging;
using MDIP.Core.Domain.Configs;
using MDIP.Core.Domain.Configuration;

namespace MDIP.Application.Services.Global.Configuration;

public class ConfigService : IConfigService
{
    private readonly Dictionary<string, ConfigItem> _modules = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ConfigItem> _pathIndex = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, FileSystemWatcher> _watchers = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Timer> _debounceTimers = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _syncRoot = new();

    private string _configDirectory;
    private bool _watchersActivated;

    private static readonly TimeSpan DebounceDelay = TimeSpan.FromMilliseconds(400);
    private const int MaxRetryAttempts = 6;
    private const int InitialRetryDelayMs = 100;
    private const double RetryBackoffFactor = 1.8;

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

            foreach (var timer in _debounceTimers.Values)
                timer.Dispose();

            _configDirectory = null;
            _modules.Clear();
            _pathIndex.Clear();
            _watchers.Clear();
            _debounceTimers.Clear();
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
                Logger.Error($"Module {name} already exists.");
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
            _pathIndex[configPath] = module;
        }
    }

    public T GetConfig<T>(string moduleName) where T : ConfigBase, new()
    {
        lock (_syncRoot)
        {
            return !_modules.TryGetValue(moduleName, out var module) ? throw new InvalidOperationException($"Module {moduleName} not found.") : module.GetConfig<T>();
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
        return Path.GetFullPath(Path.Combine(directory, fileName));
    }

    private string EnsureConfigDirectory()
    {
        if (!string.IsNullOrEmpty(_configDirectory))
            return _configDirectory;

        var directory = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "UserData", "Info+"));
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            Logger.Info("Created config directory.");
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
        lock (_syncRoot)
        {
            if (_pathIndex.ContainsKey(e.OldFullPath))
            {
                Logger.Fatal($"Config file renamed from {e.OldName} to {e.Name}. Config file renaming is not allowed. The module will not be reloaded.");
            }
            else
            {
                Logger.Warn($"Unrecognized config file renamed: {e.OldFullPath} → {e.FullPath}");
            }
        }
    }

    private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        lock (_syncRoot)
        {
            if (!_pathIndex.ContainsKey(e.FullPath))
                return;

            if (_debounceTimers.TryGetValue(e.FullPath, out var timer))
                timer.Change(DebounceDelay, Timeout.InfiniteTimeSpan);
            else
            {
                var t = new Timer(static s =>
                {
                    var self = (ConfigService)((object[])s)?[0];
                    var path = (string)((object[])s)?[1];
                    self?.OnDebouncedConfigChanged(path);
                }, new object[] { this, e.FullPath }, DebounceDelay, Timeout.InfiniteTimeSpan);
                _debounceTimers[e.FullPath] = t;
            }
        }
    }

    private void OnDebouncedConfigChanged(string path)
    {
        ConfigItem module;
        string displayName;

        lock (_syncRoot)
        {
            if (_debounceTimers.TryGetValue(path, out var timer))
            {
                timer.Dispose();
                _debounceTimers.Remove(path);
            }

            if (!_pathIndex.TryGetValue(path, out module))
                return;

            displayName = Path.GetFileName(path);
        }

        var attempt = 0;
        var delay = InitialRetryDelayMs;

        while (true)
        {
            try
            {
                module.ReloadConfig();
                Logger.Info($"Reloaded: {displayName}");
                return;
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (Exception ex)
            {
                Logger.Error($"Error reloading: {displayName}");
                Logger.Error(ex);
                return;
            }

            attempt++;
            if (attempt > MaxRetryAttempts)
            {
                Logger.Error($"Failed to reload after {MaxRetryAttempts} attempts: {displayName}");
                return;
            }

            Thread.Sleep(delay);
            delay = Math.Max(delay + 1, (int)(delay * RetryBackoffFactor));
        }
    }

    [UsedImplicitly] [Inject] public ILogger<ConfigService> Logger { get; set; }
}