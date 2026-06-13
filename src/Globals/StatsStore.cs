using System.Text.Json;
using MDIP.Core.Constants;
using MDIP.Core.Domain.Stats;

namespace MDIP.Globals;

// The persistent personal-best stats cache (info+_song_stats.json), keyed by the content-derived
// song hash (see MusicInfoUtils.CurMusicHash). The key is intentionally NOT the uid: a custom
// chart's uid is load-order/per-client dependent, so it would orphan history across sessions.
internal static class StatsStore
{
    private static readonly object SyncRoot = new();
    private static Dictionary<string, StatsData> _statsDict;

    public static StatsData GetStats(string song)
    {
        if (string.IsNullOrWhiteSpace(song))
            return null;

        lock (SyncRoot)
        {
            EnsureLoaded();
            _statsDict.TryGetValue(song, out var stats);
            return stats;
        }
    }

    public static void SetStats(string song, StatsData stats)
    {
        if (string.IsNullOrWhiteSpace(song))
            return;

        lock (SyncRoot)
        {
            EnsureLoaded();
            _statsDict[song] = stats;
            SaveToFile();
        }
    }

    private static void EnsureLoaded()
    {
        if (_statsDict != null)
            return;
        LoadFromFile();
    }

    private static void LoadFromFile()
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
                Log.Error("Error loading stats data.");
                Log.Error(ex);
                _statsDict = new();
            }
        }
        else
            _statsDict = new();
    }

    private static void SaveToFile()
    {
        try
        {
            var directory = Path.GetDirectoryName(Constants.STATS_DATA_FILE);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var jsonString = JsonSerializer.Serialize(_statsDict);
            File.WriteAllText(Constants.STATS_DATA_FILE, jsonString);
        }
        catch (Exception ex)
        {
            Log.Error("Error saving stats data.");
            Log.Error(ex);
        }
    }
}
