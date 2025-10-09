using System.Text.Json;
using JetBrains.Annotations;
using MDIP.Application.DependencyInjection;
using MDIP.Application.Services.Logging;
using MDIP.Domain.Stats;
using MDIP.Utils;

namespace MDIP.Application.Services.Stats;

public class StatsSaverService : IStatsSaverService
{
    private readonly object _syncRoot = new();
    private Dictionary<string, StatsData> _statsDict;

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
                Logger.Error($"Error loading stats data: {ex.Message}");
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
            Logger.Error($"Error saving stats data: {ex.Message}");
        }
    }

    [UsedImplicitly] [Inject] public ILogger<StatsSaverService> Logger { get; set; }
}