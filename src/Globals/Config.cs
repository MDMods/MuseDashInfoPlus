using MDIP.Core.Domain.Configs;
using MDIP.Core.Domain.Configuration;

namespace MDIP.Globals;

// The configuration store + typed accessors, merged into one place (the old ConfigService +
// ConfigAccessor). Owns the YAML modules, their file watchers and the debounced hot-reload. File
// formats and the per-module schema are unchanged from v3.0.x, so the existing version-control
// migration carries user values forward on a version bump with no extra work.
internal static class Config
{
    private static readonly Dictionary<string, ConfigItem> Modules = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, ConfigItem> PathIndex = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, FileSystemWatcher> Watchers = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, Timer> DebounceTimers = new(StringComparer.OrdinalIgnoreCase);
    private static readonly object SyncRoot = new();

    private static string _configDirectory;
    private static bool _watchersActivated;

    private static readonly TimeSpan DebounceDelay = TimeSpan.FromMilliseconds(400);
    private const int MaxRetryAttempts = 6;
    private const int InitialRetryDelayMs = 100;
    private const double RetryBackoffFactor = 1.8;

    // ───────── typed accessors (the old ConfigAccessor) ─────────
    public static MainConfigs Main => GetConfig<MainConfigs>(nameof(MainConfigs));
    public static AdvancedConfigs Advanced => GetConfig<AdvancedConfigs>(nameof(AdvancedConfigs));
    public static TextFieldLowerLeftConfigs TextFieldLowerLeft => GetConfig<TextFieldLowerLeftConfigs>(nameof(TextFieldLowerLeftConfigs));
    public static TextFieldLowerRightConfigs TextFieldLowerRight => GetConfig<TextFieldLowerRightConfigs>(nameof(TextFieldLowerRightConfigs));
    public static TextFieldScoreBelowConfigs TextFieldScoreBelow => GetConfig<TextFieldScoreBelowConfigs>(nameof(TextFieldScoreBelowConfigs));
    public static TextFieldScoreRightConfigs TextFieldScoreRight => GetConfig<TextFieldScoreRightConfigs>(nameof(TextFieldScoreRightConfigs));
    public static TextFieldUpperLeftConfigs TextFieldUpperLeft => GetConfig<TextFieldUpperLeftConfigs>(nameof(TextFieldUpperLeftConfigs));
    public static TextFieldUpperRightConfigs TextFieldUpperRight => GetConfig<TextFieldUpperRightConfigs>(nameof(TextFieldUpperRightConfigs));

    public static void Init()
    {
        lock (SyncRoot)
        {
            foreach (var watcher in Watchers.Values)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Changed -= OnConfigFileChanged;
                watcher.Created -= OnConfigFileChanged;
                watcher.Renamed -= OnConfigFileRenamed;
                watcher.Dispose();
            }

            foreach (var timer in DebounceTimers.Values)
                timer.Dispose();

            _configDirectory = null;
            Modules.Clear();
            PathIndex.Clear();
            Watchers.Clear();
            DebounceTimers.Clear();
            _watchersActivated = false;
        }
    }

    public static void ActivateWatcher()
    {
        lock (SyncRoot)
        {
            foreach (var watcher in Watchers.Values)
                watcher.EnableRaisingEvents = true;
            _watchersActivated = true;
        }
    }

    public static void RegisterModule(string name, string configFileName)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Module name cannot be null or empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(configFileName))
            throw new ArgumentException("Config file name cannot be null or empty.", nameof(configFileName));

        lock (SyncRoot)
        {
            if (Modules.ContainsKey(name))
            {
                Log.Error($"Module {name} already exists.");
                return;
            }

            var configPath = GetConfigFilePath(configFileName);
            var directory = Path.GetDirectoryName(configPath);
            if (string.IsNullOrWhiteSpace(directory))
                throw new DirectoryNotFoundException($"Configs directory {configFileName} not found.");

            if (!Watchers.ContainsKey(directory))
            {
                var watcher = CreateWatcher(directory);
                Watchers.Add(directory, watcher);
                if (_watchersActivated)
                    watcher.EnableRaisingEvents = true;
            }

            var module = new ConfigItem(name, configPath);
            Modules.Add(name, module);
            PathIndex[configPath] = module;
        }
    }

    public static T GetConfig<T>(string moduleName) where T : ConfigBase, new()
    {
        lock (SyncRoot)
        {
            return !Modules.TryGetValue(moduleName, out var module)
                ? throw new InvalidOperationException($"Module {moduleName} not found.")
                : module.GetConfig<T>();
        }
    }

    public static void SaveConfig<T>(string moduleName, T config) where T : ConfigBase
    {
        lock (SyncRoot)
        {
            if (!Modules.TryGetValue(moduleName, out var module))
                throw new InvalidOperationException($"Module {moduleName} not found.");
            module.SaveConfig(config);
        }
    }

    public static void RegisterUpdateCallback<T>(string moduleName, Action<T> callback) where T : class
    {
        if (callback == null)
            return;

        lock (SyncRoot)
        {
            if (!Modules.TryGetValue(moduleName, out var module))
                throw new InvalidOperationException($"Module {moduleName} not found.");
            module.RegisterUpdateCallback(callback);
        }
    }

    public static string GetConfigFilePath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));

        var directory = EnsureConfigDirectory();
        return Path.GetFullPath(Path.Combine(directory, fileName));
    }

    private static string EnsureConfigDirectory()
    {
        if (!string.IsNullOrEmpty(_configDirectory))
            return _configDirectory;

        var directory = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "UserData", "Info+"));
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            Log.Info("Created config directory.");
        }

        _configDirectory = directory;
        return directory;
    }

    private static FileSystemWatcher CreateWatcher(string directory)
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

    private static void OnConfigFileRenamed(object sender, RenamedEventArgs e)
    {
        lock (SyncRoot)
        {
            if (PathIndex.ContainsKey(e.OldFullPath))
                Log.Fatal($"Config file renamed from {e.OldName} to {e.Name}. Config file renaming is not allowed. The module will not be reloaded.");
            else
                Log.Warn($"Unrecognized config file renamed: {e.OldFullPath} → {e.FullPath}");
        }
    }

    private static void OnConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        lock (SyncRoot)
        {
            if (!PathIndex.ContainsKey(e.FullPath))
                return;

            if (DebounceTimers.TryGetValue(e.FullPath, out var timer))
                timer.Change(DebounceDelay, Timeout.InfiniteTimeSpan);
            else
                DebounceTimers[e.FullPath] = new Timer(static s => OnDebouncedConfigChanged((string)s), e.FullPath, DebounceDelay, Timeout.InfiniteTimeSpan);
        }
    }

    private static void OnDebouncedConfigChanged(string path)
    {
        ConfigItem module;
        string displayName;

        lock (SyncRoot)
        {
            if (DebounceTimers.TryGetValue(path, out var timer))
            {
                timer.Dispose();
                DebounceTimers.Remove(path);
            }

            if (!PathIndex.TryGetValue(path, out module))
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
                Log.Info($"Reloaded: {displayName}");
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
                Log.Error($"Error reloading: {displayName}");
                Log.Error(ex);
                return;
            }

            attempt++;
            if (attempt > MaxRetryAttempts)
            {
                Log.Error($"Failed to reload after {MaxRetryAttempts} attempts: {displayName}");
                return;
            }

            Thread.Sleep(delay);
            delay = Math.Max(delay + 1, (int)(delay * RetryBackoffFactor));
        }
    }
}
