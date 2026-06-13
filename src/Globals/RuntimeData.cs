namespace MDIP.Globals;

// Process-lifetime UI state. Just the desired-visibility toggle now: personal bests are read live from
// each ecosystem's authoritative store (see ChartSource), so there is no PB cache to drift.
internal static class RuntimeData
{
    // The player's overlay on/off choice. Persists across songs; initialized once from config at
    // startup (InitDesiredUiVisible) — never lazily during the battle, so it is correct from frame 0.
    public static bool DesiredUiVisible { get; private set; }

    public static void InitDesiredUiVisible(bool visible) => DesiredUiVisible = visible;
    public static void SetDesiredUiVisible(bool visible) => DesiredUiVisible = visible;
}
