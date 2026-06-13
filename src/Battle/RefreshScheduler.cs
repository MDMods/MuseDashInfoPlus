using MDIP.Globals;

namespace MDIP.Battle;

// Drives the per-frame and per-interval refreshes during a battle. Owned by a BattleSession and only
// ticked while that session is alive, so it needs no liveness gating of its own.
public class RefreshScheduler(BattleUi ui, GameStats stats, TextObjects textObjects)
{
    private long _lastDataMs;
    private long _lastTextMs;

    private int _lastReportSecond = -1;
    private int _dataTriggerCount;
    private int _textTriggerCount;

    public void OnFixedUpdateTick()
    {
        ui.CheckAndZoom();
    }

    public void OnLateUpdateTick()
    {
        TextData.ApplyPendingConstantsRefresh(stats);
        ui.ApplyPendingConfigChanges();

        // Hotkey polling: only when in-level and native zoom-in completed
        if (Config.Main.EnableUiToggleHotkey &&
            stats.IsPlayerPlaying &&
            ui.NativeZoomInCompleted)
        {
            if (Hotkeys.CheckToggleTriggered())
                ui.SetDesiredUiVisible(!RuntimeData.DesiredUiVisible);
        }

        if (!stats.IsPlayerPlaying)
            return;

        var now = NowMs();

        var dataInterval = Math.Max(0, Config.Advanced.DataRefreshIntervalMs);
        var textInterval = Config.Advanced.TextRefreshIntervalMs <= 0 ? dataInterval : Config.Advanced.TextRefreshIntervalMs;
        if (textInterval < dataInterval) textInterval = dataInterval;

        if (now - _lastDataMs >= dataInterval)
        {
            _lastDataMs = now;
            stats.UpdateCurrentStats();
            _dataTriggerCount++;
        }

        if (now - _lastTextMs >= textInterval)
        {
            _lastTextMs = now;
            textObjects.UpdateAllText();
            _textTriggerCount++;
        }

        if (Config.Advanced.DisplayNoteDebuggingData)
            ReportPerSecond();
    }

    private static long NowMs()
        => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    private void ReportPerSecond()
    {
        var nowSec = DateTime.Now.Second;
        if (_lastReportSecond == nowSec)
            return;
        _lastReportSecond = nowSec;

        Log.Info($"Trigger counts: data({_dataTriggerCount}) text({_textTriggerCount})");

        _dataTriggerCount = 0;
        _textTriggerCount = 0;
    }
}
