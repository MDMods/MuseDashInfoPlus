using Il2CppAssets.Scripts.UI.Panels;
using MDIP.Globals;

namespace MDIP.Battle;

// The coordinator every Harmony patch and the mod lifecycle forward to. It owns the single live
// BattleSession, and Guard is the ONE exception-isolation boundary — every entry runs through it, so
// a fault inside Info+ can never propagate into the game's call chain or another mod's patch.
//
// The session is created on PnlBattle.GameStart (postfix) — the moment the native battle UI is set up
// and begins its own zoom-in. Info+'s text is parented under that panel, so it rides the game's
// built-in entrance for free (no Info+ animation, no delay). A single __runOriginal guard handles
// multiplayer: a barrier can suppress PnlBattle.GameStart to freeze the player at frame 0 and
// re-invoke at the synced start; the postfix still runs on the suppressed call, but the native UI
// isn't set up then — so we act only when the original actually ran (the real start, where the panel
// and its zoom exist to ride).
internal static class BattleController
{
    public static BattleSession Current { get; private set; }

    public static void OnBattleStart(PnlBattle pnl, bool runOriginal) => Guard(() =>
    {
        if (!runOriginal || pnl == null)
            return;

        // Clean-slate guard — normally a no-op because the previous session was disposed at the last
        // Loading scene.
        DisposeCurrent();
        Current = new BattleSession(pnl);
    });

    // Battle/scene teardown (the "Loading" scene). The game tears down the native battle scene itself.
    public static void EndSession() => Guard(DisposeCurrent);

    public static void OnFixedUpdate() => Guard(() => Current?.Scheduler.OnFixedUpdateTick());

    public static void OnLateUpdate() => Guard(() => Current?.Scheduler.OnLateUpdateTick());

    public static void OnSetPlayResult(int idx, byte result, bool isMulStart, bool isMulEnd, bool isLeft)
        => Guard(() => Current?.NoteEvents.HandleSetPlayResult(idx, result, isMulStart, isMulEnd, isLeft));

    public static void OnMissCube(int idx, decimal currentTick)
        => Guard(() => Current?.NoteEvents.HandleMissCube(idx, currentTick));

    // PnlVictory.SetDetailInfo postfix. No live session => nothing to do (the game owns the score/acc
    // PB; without a run there is no breakdown to record).
    public static void OnVictoryDetail(PnlVictory instance)
        => Guard(() => Current?.Victory.OnSetDetailInfo(instance));

    public static void OnControllerMissRecord(BaseEnemyObjectController instance) => Guard(() =>
    {
        if (!Config.Advanced.OutputNoteRecordsToDesktop)
            return;
        Current?.NoteRecords.AddRecord(instance.m_MusicData, "ControllerMissCheck", $"m_HasMiss:{instance.m_HasMiss}");
    });

    public static void OnMultHitMissRecord(MultHitEnemyController instance) => Guard(() =>
    {
        if (!Config.Advanced.OutputNoteRecordsToDesktop)
            return;
        Current?.NoteRecords.AddRecord(instance.m_MusicData, "OnControllerMiss", $"m_HasMiss:{instance.m_HasMiss}");
    });

    public static void OnNoteResultRecord(int result) => Guard(() =>
    {
        if (!Config.Advanced.OutputNoteRecordsToDesktop || Current == null)
            return;
        Current.NoteRecords.AddRecord(Current.Stats.GetCurMusicData(), "OnNoteResult", $"noteResult:{result}");
    });

    // A config file changed: ask the live overlay (if any) to re-apply on its next tick.
    public static void QueueConfigApply() => Guard(() => Current?.Ui.QueueApplyConfigChanges());

    private static void DisposeCurrent()
    {
        Current?.Dispose();
        Current = null;
    }

    // The single exception-isolation boundary: an Info+ fault is logged and swallowed here, never
    // propagated into the game's call chain.
    private static void Guard(Action body)
    {
        try
        {
            body();
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }
}
