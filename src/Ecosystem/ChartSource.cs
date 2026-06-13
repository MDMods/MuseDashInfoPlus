using Il2CppAssets.Scripts.Database;

namespace MDIP.Ecosystem;

// The mod's single chart-source branch: "where is this chart's personal best?".
//
// Every ecosystem (vanilla, CustomAlbums, Euterpe) injects its charts as real MusicInfo entries, so
// title/author/difficulty/level/album all read uniformly from the game — only the PB store diverges:
//   - Euterpe keeps it in its own save, read through EuterpeBridgeClient;
//   - vanilla AND CustomAlbums use the native high-score store (CAM writes there too), so they share
//     the one fallback. No CAM-specific code exists because CAM needs none today — and adding a
//     detect-but-do-nothing branch would be dead weight.
internal static class ChartSource
{
    // Returns (native game score, accuracy as 0..100); accuracy is 0 when the source has none, and
    // the caller maxes it against its own session cache.
    public static (int Score, float AccuracyPercent) ResolvePersonalBest(MusicInfo info, int difficulty)
    {
        if (info != null
            && EuterpeBridgeClient.IsEuterpeChart(info.uid)
            && EuterpeBridgeClient.TryGetPersonalBest(info.uid, difficulty, out var score, out var accuracyPercent))
            return (score, accuracyPercent);

        return (BattleHelper.GetCurrentMusicHighScore(), 0f);
    }
}
