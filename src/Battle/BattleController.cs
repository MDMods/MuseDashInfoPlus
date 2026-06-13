using Il2CppAssets.Scripts.GameCore.HostComponent;
using Il2CppAssets.Scripts.UI.Panels;
using Il2CppGameLogic;
using MDIP.Core.Utilities;
using MDIP.Globals;

namespace MDIP.Battle;

// The coordinator every Harmony patch and the mod lifecycle forward to. It owns the single live
// BattleSession, and Guard is the ONE exception-isolation boundary — every entry runs through it, so
// a fault inside Info+ can never propagate into the game's call chain or another mod's patch.
//
// The session is created on StageBattleComponent.GameStart — the simulation's real start, which fires
// exactly once per run and downstream of PnlBattle.GameStart. Hooking it (instead of the UI panel's
// GameStart) means Info+ does NOT contend with mods that gate PnlBattle.GameStart — e.g. a
// multiplayer barrier that freezes the player at frame 0 and re-invokes at the synced start: that
// barrier suppresses PnlBattle.GameStart, so StageBattleComponent.GameStart simply doesn't fire until
// the real start. No __runOriginal guard, no double-fire handling, no contention to defend against.
internal static class BattleController
{
    public static BattleSession Current { get; private set; }

    // StageBattleComponent.GameStart postfix. Builds the session for this run; the overlay reads the
    // live battle panel via PnlBattle.instance. DisposeCurrent is a clean-slate guard — normally a
    // no-op because the previous session was disposed at the last Loading scene.
    public static void OnBattleStart() => Guard(() =>
    {
        var pnl = PnlBattle.instance;
        if (pnl == null)
            return;

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

    public static void OnVictoryDetail(PnlVictory instance) => Guard(() =>
    {
        if (Current != null)
        {
            Current.Victory.OnSetDetailInfo(instance);
            return;
        }

        // No live session (the scene already tore down before the results screen): persist the best
        // directly from the game data.
        SavePersonalBestDirect();
    });

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

    public static void OnPrepRecordUpdated(PnlPreparation instance)
        => Guard(() => PrepScreen.OnRecordUpdated(instance));

    // A config file changed: ask the live overlay (if any) to re-apply on its next tick.
    public static void QueueConfigApply() => Guard(() => Current?.Ui.QueueApplyConfigChanges());

    private static void DisposeCurrent()
    {
        Current?.Dispose();
        Current = null;
    }

    private static void SavePersonalBestDirect()
    {
        var task = TaskStageTarget.instance;
        if (task == null)
            return;

        var hash = MusicInfoUtils.CurMusicHash;
        var acc = (float)Math.Round((double)(task.GetAccuracy() * 100f), 2);
        var score = task.m_Score;

        RuntimeData.StorePersonalBestAccuracy(hash, acc);
        RuntimeData.StorePersonalBestScore(hash, score);
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
