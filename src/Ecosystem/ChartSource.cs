using System.Reflection;
using Il2CppAssets.Scripts.Database;
using MDIP.Globals;

namespace MDIP.Ecosystem;

public enum ChartEcosystem
{
    Vanilla,
    CustomAlbums,
    Euterpe
}

// The single place that answers "which ecosystem is this chart, and where is its personal best?".
//
// Title/author/difficulty/level/album all read uniformly from the game's MusicInfo, because every
// ecosystem (vanilla, CustomAlbums, Euterpe) injects its charts as real MusicInfo entries — so only
// the PERSONAL BEST diverges:
//   - vanilla + CustomAlbums keep the best in the native high-score store (CAM writes there);
//   - Euterpe keeps it in its own save, exposed via the reflection bridge Euterpe.EuterpeBridge.
//
// The Euterpe bridge is a soft dependency: when Euterpe is absent (or too old to expose it) the type
// is simply missing and Euterpe detection/PB silently fall through to the vanilla path.
internal static class ChartSource
{
    // CustomAlbums injects its charts under the synthetic album json "ALBUM1000" (AlbumManager.Uid
    // 999 + 1), and every CAM uid is "999-{index}". Either fingerprint identifies a CAM chart on the
    // game's MusicInfo. (Detection only — CAM's PB path is the native store, identical to vanilla.)
    private const int CustomAlbumsJsonIndex = 1000;
    private const string CustomAlbumsUidPrefix = "999-";

    private static bool _bridgeResolved;
    private static MethodInfo _isEuterpeUid;
    private static MethodInfo _tryGetLocalBest;

    public static ChartEcosystem GetEcosystem(MusicInfo info)
    {
        if (info == null)
            return ChartEcosystem.Vanilla;

        var uid = info.uid;
        if (IsEuterpeUid(uid))
            return ChartEcosystem.Euterpe;

        if (info.albumJsonIndex == CustomAlbumsJsonIndex ||
            (!string.IsNullOrEmpty(uid) && uid.StartsWith(CustomAlbumsUidPrefix, StringComparison.Ordinal)))
            return ChartEcosystem.CustomAlbums;

        return ChartEcosystem.Vanilla;
    }

    // Resolves the loaded personal best for the chart + 1-based difficulty. Score is the native game
    // score; accuracy is 0..100 (percent) to match HistoryStats.Accuracy, or 0 when the source has no
    // accuracy to offer (the caller maxes it against its own session cache).
    public static (int Score, float AccuracyPercent) ResolvePersonalBest(MusicInfo info, int difficulty)
    {
        if (info == null)
            return (0, 0f);

        // Euterpe charts: the native store has nothing — query Euterpe's own save through the bridge.
        if (IsEuterpeUid(info.uid) && TryEuterpePersonalBest(info.uid, difficulty, out var score, out var accuracyPercent))
            return (score, accuracyPercent);

        // Vanilla and CustomAlbums: the best lives in the native high-score store.
        return (BattleHelper.GetCurrentMusicHighScore(), 0f);
    }

    private static bool IsEuterpeUid(string uid)
    {
        EnsureBridge();
        if (_isEuterpeUid == null || string.IsNullOrEmpty(uid))
            return false;

        try
        {
            return (bool)_isEuterpeUid.Invoke(null, [uid]);
        }
        catch (Exception ex)
        {
            Log.Warn($"ChartSource: Euterpe IsEuterpeUid failed: {ex.Message}");
            return false;
        }
    }

    private static bool TryEuterpePersonalBest(string uid, int difficulty, out int score, out float accuracyPercent)
    {
        score = 0;
        accuracyPercent = 0f;

        EnsureBridge();
        if (_tryGetLocalBest == null)
            return false;

        try
        {
            var args = new object[] { uid, difficulty, 0, 0f };
            if (!(bool)_tryGetLocalBest.Invoke(null, args))
                return false;

            score = (int)args[2];
            accuracyPercent = (float)args[3] * 100f; // the bridge returns accuracy as 0..1
            return true;
        }
        catch (Exception ex)
        {
            Log.Warn($"ChartSource: Euterpe TryGetLocalBest failed: {ex.Message}");
            return false;
        }
    }

    // Resolves Euterpe.EuterpeBridge once (by exact full name, across loaded assemblies) and caches
    // its two methods. Absent type => Euterpe not installed (or too old): stays null, callers fall
    // through to the vanilla path.
    private static void EnsureBridge()
    {
        if (_bridgeResolved)
            return;
        _bridgeResolved = true;

        try
        {
            Type type = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType("Euterpe.EuterpeBridge", false);
                if (type != null)
                    break;
            }

            if (type == null)
            {
                Log.Info("ChartSource: Euterpe bridge not present; custom-chart PB limited to vanilla/CAM sources.");
                return;
            }

            _isEuterpeUid = type.GetMethod("IsEuterpeUid", [typeof(string)]);
            _tryGetLocalBest = type.GetMethod("TryGetLocalBest",
                [typeof(string), typeof(int), typeof(int).MakeByRefType(), typeof(float).MakeByRefType()]);

            Log.Info($"ChartSource: Euterpe bridge resolved (IsEuterpeUid={_isEuterpeUid != null}, TryGetLocalBest={_tryGetLocalBest != null}).");
        }
        catch (Exception ex)
        {
            Log.Warn($"ChartSource: Euterpe bridge resolution failed: {ex.Message}");
        }
    }
}
