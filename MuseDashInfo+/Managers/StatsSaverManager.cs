using System.Text.Json;

namespace MDIP.Managers;

public static class StatsSaverManager
{
    private static Dictionary<string, StatsData> _statsDict;

    public static StatsData GetStats(string song)
    {
        if (_statsDict == null || _statsDict.Count < 1)
            LoadFromFile();
        return _statsDict?.GetValueOrDefault(song);
    }

    public static void SetStats(string song, StatsData stats)
    {
        _statsDict[song] = stats;
        SaveToFile();
    }

    private static void LoadFromFile()
    {
        if (File.Exists(Constants.STATS_DATA_FILE))
        {
            try
            {
                var jsonString = File.ReadAllText(Constants.STATS_DATA_FILE);
                _statsDict = JsonSerializer.Deserialize<Dictionary<string, StatsData>>(jsonString)
                             ?? new Dictionary<string, StatsData>();
            }
            catch (Exception ex)
            {
                Melon<MDIPMod>.Logger.Error($"Error loading stats data: {ex.Message}");
                _statsDict = new();
            }
        }
        else
        {
            _statsDict = new();
        }
    }

    private static void SaveToFile()
    {
        try
        {
            var jsonString = JsonSerializer.Serialize(_statsDict,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });
            File.WriteAllText(Constants.STATS_DATA_FILE, jsonString);
        }
        catch (Exception ex)
        {
            Melon<MDIPMod>.Logger.Error($"Error saving stats data: {ex.Message}");
        }
    }
}