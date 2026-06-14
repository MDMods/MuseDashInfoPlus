using System.Reflection;
using MDIP.Globals;

namespace MDIP.Ecosystem;

// Anti-corruption layer to the Euterpe mod's pinned cross-mod contract (Euterpe.EuterpeBridge),
// reached by reflection as a SOFT dependency: if Euterpe is absent (or too old to expose the bridge)
// the type is simply missing and every call reports "no data", so callers fall back to the vanilla
// path. ALL the reflection mechanics — type resolution, method caching, out-parameter marshalling,
// the 0..1 -> 0..100 accuracy conversion — live here; nothing else in the mod knows Euterpe is
// reached reflectively.
internal static class EuterpeBridgeClient
{
    private static bool _resolved;
    private static MethodInfo _isEuterpeUid;
    private static MethodInfo _tryGetLocalBest;
    private static MethodInfo _isMultiplayerActive;

    // True while the current battle is a Euterpe multiplayer round — Euterpe paints its own in-battle
    // heads-up then, so Info+ retreats from the corners it occupies (see BattleUi's bindings). Soft:
    // a missing bridge (Euterpe absent / too old) reports false, so non-MP play is never affected.
    public static bool IsMultiplayerActive()
    {
        EnsureResolved();
        if (_isMultiplayerActive == null)
            return false;

        try
        {
            return (bool)_isMultiplayerActive.Invoke(null, null);
        }
        catch (Exception ex)
        {
            Log.Warn($"EuterpeBridge IsMultiplayerActive failed: {ex.Message}");
            return false;
        }
    }

    public static bool IsEuterpeChart(string uid)
    {
        EnsureResolved();
        if (_isEuterpeUid == null || string.IsNullOrEmpty(uid))
            return false;

        try
        {
            return (bool)_isEuterpeUid.Invoke(null, [uid]);
        }
        catch (Exception ex)
        {
            Log.Warn($"EuterpeBridge IsEuterpeUid failed: {ex.Message}");
            return false;
        }
    }

    // Accuracy is returned as 0..100 (percent) to match HistoryStats.Accuracy; the bridge gives 0..1.
    public static bool TryGetPersonalBest(string uid, int difficulty, out int score, out float accuracyPercent)
    {
        score = 0;
        accuracyPercent = 0f;

        EnsureResolved();
        if (_tryGetLocalBest == null)
            return false;

        try
        {
            var args = new object[] { uid, difficulty, 0, 0f };
            if (!(bool)_tryGetLocalBest.Invoke(null, args))
                return false;

            score = (int)args[2];
            accuracyPercent = (float)args[3] * 100f;
            return true;
        }
        catch (Exception ex)
        {
            Log.Warn($"EuterpeBridge TryGetLocalBest failed: {ex.Message}");
            return false;
        }
    }

    // Resolves Euterpe.EuterpeBridge once (by exact full name, across loaded assemblies) and caches
    // its two methods. Absent type => Euterpe not installed (or too old): stays null.
    private static void EnsureResolved()
    {
        if (_resolved)
            return;
        _resolved = true;

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
                Log.Info("EuterpeBridge: not present; custom-chart PB limited to the native/CAM source.");
                return;
            }

            _isEuterpeUid = type.GetMethod("IsEuterpeUid", [typeof(string)]);
            _tryGetLocalBest = type.GetMethod("TryGetLocalBest",
                [typeof(string), typeof(int), typeof(int).MakeByRefType(), typeof(float).MakeByRefType()]);
            _isMultiplayerActive = type.GetMethod("IsMultiplayerActive", Type.EmptyTypes);

            Log.Info($"EuterpeBridge: resolved (IsEuterpeUid={_isEuterpeUid != null}, " +
                     $"TryGetLocalBest={_tryGetLocalBest != null}, IsMultiplayerActive={_isMultiplayerActive != null}).");
        }
        catch (Exception ex)
        {
            Log.Warn($"EuterpeBridge: resolution failed: {ex.Message}");
        }
    }
}
