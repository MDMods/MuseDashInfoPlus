using System.Text.Json;

namespace MDIP.Services;

public class StatsSaverService : IStatsSaverService
{
    private readonly MelonLogger.Instance _logger;
    private readonly object _syncRoot = new();
    private Dictionary<string, StatsData> _statsDict;

    public StatsSaverService(MelonLogger.Instance logger) => _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public StatsData GetStats(string song)
    {
        if (string.IsNullOrWhiteSpace(song))
            return null;

        lock (_syncRoot)
        {
            EnsureLoaded();
            _statsDict.TryGetValue(song, out var stats);
            return stats;
        }
    }

    public void SetStats(string song, StatsData stats)
    {
        if (string.IsNullOrWhiteSpace(song))
            return;

        lock (_syncRoot)
        {
            EnsureLoaded();
            _statsDict[song] = stats;
            SaveToFile();
        }
    }

    private void EnsureLoaded()
    {
        if (_statsDict != null)
            return;

        LoadFromFile();
    }

    private void LoadFromFile()
    {
        if (File.Exists(Constants.STATS_DATA_FILE))
        {
            try
            {
                var jsonString = File.ReadAllText(Constants.STATS_DATA_FILE);
                _statsDict = JsonSerializer.Deserialize<Dictionary<string, StatsData>>(jsonString) ?? new Dictionary<string, StatsData>();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error loading stats data: {ex.Message}");
                _statsDict = new();
            }
        }
        else
            _statsDict = new();
    }

    private void SaveToFile()
    {
        try
        {
            var jsonString = JsonSerializer.Serialize(_statsDict);
            File.WriteAllText(Constants.STATS_DATA_FILE, jsonString);
        }
        catch (Exception ex)
        {
            _logger.Error($"Error saving stats data: {ex.Message}");
        }
    }
}