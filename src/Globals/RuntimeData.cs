using System.Globalization;
using MDIP.Core.Domain.Records;

namespace MDIP.Globals;

// In-session, process-lifetime store: the desired-visibility toggle and per-song running bests
// (seeded from the prep screen / a completed run, read by the battle overlay). Personal-best values
// here are a session cache; the authoritative cross-session store is StatsStore + the ecosystem.
internal static class RuntimeData
{
    private static readonly Dictionary<string, SongRuntimeRecord> SongRuntimeRecords = new();

    // The player's UI on/off choice. Process-lifetime so it persists across songs; initialized once
    // from config at startup (InitDesiredUiVisible) rather than lazily during the zoom check — the
    // lazy init was what left the overlay force-hidden when another mod held the battle start.
    public static bool DesiredUiVisible { get; private set; }

    public static void InitDesiredUiVisible(bool visible) => DesiredUiVisible = visible;
    public static void SetDesiredUiVisible(bool visible) => DesiredUiVisible = visible;

    public static bool IsFirstTry(string songHash)
        => !SongRuntimeRecords.ContainsKey(songHash);

    public static SongRuntimeRecord TryGet(string songHash)
        => SongRuntimeRecords.TryGetValue(songHash, out var data) ? data : new(0, 0);

    public static void AddOrUpdate(string songHash, SongRuntimeRecord songRecord)
        => SongRuntimeRecords[songHash] = songRecord;

    public static bool StorePersonalBestAccuracyFromText(string songHash, string text)
    {
        if (string.IsNullOrEmpty(songHash) || string.IsNullOrEmpty(text))
            return false;

        var trimmed = text.TrimEnd(' ', '%');
        if (TryParseAccuracy(trimmed, out var acc) && acc > 0)
            return StorePersonalBestAccuracy(songHash, acc);

        return false;
    }

    public static bool StorePersonalBestScoreFromText(string songHash, string text)
    {
        if (string.IsNullOrEmpty(songHash) || string.IsNullOrEmpty(text))
            return false;

        if (int.TryParse(text, out var score) && score > 0)
            return StorePersonalBestScore(songHash, score);

        return false;
    }

    public static bool StorePersonalBestAccuracy(string songHash, float acc)
    {
        if (acc <= 0)
            return false;

        if (SongRuntimeRecords.TryGetValue(songHash, out var record))
        {
            if (record.PersonalBestAccuracy >= acc)
                return false;

            SongRuntimeRecords[songHash] = record with { PersonalBestAccuracy = acc };
            return true;
        }

        SongRuntimeRecords.Add(songHash, new(acc, 0));
        return true;
    }

    public static bool StorePersonalBestScore(string songHash, int score)
    {
        if (score <= 0)
            return false;

        if (SongRuntimeRecords.TryGetValue(songHash, out var record))
        {
            if (record.PersonalBestScore >= score)
                return false;

            SongRuntimeRecords[songHash] = record with { PersonalBestScore = score };
            return true;
        }

        SongRuntimeRecords.Add(songHash, new(0, score));
        return true;
    }

    private static bool TryParseAccuracy(string text, out float value)
        => float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
           || float.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
}
