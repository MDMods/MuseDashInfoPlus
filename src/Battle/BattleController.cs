using Il2CppAssets.Scripts.GameCore.HostComponent;
using Il2CppAssets.Scripts.UI.Panels;
using Il2CppGameLogic;
using MDIP.Core.Utilities;
using MDIP.Globals;

namespace MDIP.Battle;

// The coordinator every Harmony patch and the mod lifecycle forward to. It owns the single live
// BattleSession and is the ONE exception-isolation boundary: every entry point catches, logs and
// swallows, so a fault inside Info+ can never propagate into the game's call chain or another mod's
// patch (the failure mode behind the multiplayer "all native UI disappears" report).
internal static class BattleController
{
    public static BattleSession Current { get; private set; }

    // PnlBattle.GameStart postfix. Acts only when the original method actually ran (runOriginal):
    // another mod's prefix can suppress GameStart — e.g. a multiplayer barrier freezing the player at
    // frame 0 — and the held re-invocation at the synced start is the real one. Idempotent: a session
    // already live for this battle is left untouched, so a second GameStart never builds a second
    // overlay.
    public static void OnGameStart(PnlBattle pnl, bool runOriginal)
    {
        if (!runOriginal)
            return;

        try
        {
            if (Current != null)
                return;
            Current = new BattleSession(pnl);
        }
        catch (Exception ex)
        {
            Log.Error("Battle session start failed.");
            Log.Error(ex);
            EndSession();
        }
    }

    // Battle/scene teardown (the "Loading" scene). Disposing our session destroys our overlay objects;
    // the game tears down the native battle scene itself.
    public static void EndSession()
    {
        try
        {
            Current?.Dispose();
        }
        catch (Exception ex)
        {
            Log.Error("Battle session dispose failed.");
            Log.Error(ex);
        }
        finally
        {
            Current = null;
        }
    }

    public static void OnFixedUpdate()
    {
        try
        {
            Current?.Scheduler.OnFixedUpdateTick();
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    public static void OnLateUpdate()
    {
        try
        {
            Current?.Scheduler.OnLateUpdateTick();
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    public static void OnSetPlayResult(int idx, byte result, bool isMulStart, bool isMulEnd, bool isLeft)
    {
        try
        {
            Current?.NoteEvents.HandleSetPlayResult(idx, result, isMulStart, isMulEnd, isLeft);
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    public static void OnMissCube(int idx, decimal currentTick)
    {
        try
        {
            Current?.NoteEvents.HandleMissCube(idx, currentTick);
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    public static void OnVictoryDetail(PnlVictory instance)
    {
        try
        {
            if (Current != null)
            {
                Current.Victory.OnSetDetailInfo(instance);
                return;
            }

            // Fallback: the session is already gone (e.g. the scope-free teardown ran on a scene
            // transition before the results screen). Persist the best directly from the game data.
            SavePersonalBestDirect();
        }
        catch (Exception ex)
        {
            Log.Error("Victory detail handling failed.");
            Log.Error(ex);
        }
    }

    public static void OnControllerMissRecord(BaseEnemyObjectController instance)
    {
        try
        {
            if (!Config.Advanced.OutputNoteRecordsToDesktop)
                return;
            Current?.NoteRecords.AddRecord(instance.m_MusicData, "ControllerMissCheck", $"m_HasMiss:{instance.m_HasMiss}");
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    public static void OnMultHitMissRecord(MultHitEnemyController instance)
    {
        try
        {
            if (!Config.Advanced.OutputNoteRecordsToDesktop)
                return;
            Current?.NoteRecords.AddRecord(instance.m_MusicData, "OnControllerMiss", $"m_HasMiss:{instance.m_HasMiss}");
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    public static void OnNoteResultRecord(int result)
    {
        try
        {
            if (!Config.Advanced.OutputNoteRecordsToDesktop || Current == null)
                return;
            Current.NoteRecords.AddRecord(Current.Stats.GetCurMusicData(), "OnNoteResult", $"noteResult:{result}");
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    public static void OnPrepRecordUpdated(PnlPreparation instance)
    {
        try
        {
            PrepScreen.OnRecordUpdated(instance);
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    // A config file changed: ask the live overlay (if any) to re-apply on its next tick. No-op in
    // menus, where there is no session.
    public static void QueueConfigApply()
    {
        try
        {
            Current?.Ui.QueueApplyConfigChanges();
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
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
}
