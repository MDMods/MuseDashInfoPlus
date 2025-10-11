using JetBrains.Annotations;
using MDIP.Application.DependencyInjection;
using MDIP.Application.Services.Global.Configuration;
using MDIP.Application.Services.Global.Input;
using MDIP.Application.Services.Global.Logging;
using MDIP.Application.Services.Scoped.Stats;
using MDIP.Application.Services.Scoped.Text;
using MDIP.Application.Services.Scoped.UI;

namespace MDIP.Application.Services.Scoped.Scheduling;

public class RefreshScheduler : IRefreshScheduler
{
    private long _lastDataMs;
    private long _lastTextMs;

    private int _lastReportSecond = -1;
    private int _dataTriggerCount;
    private int _textTriggerCount;

    public void OnFixedUpdateTick()
    {
        if (ModServiceConfigurator.Provider == null || ModServiceConfigurator.CurrentScope == null)
            return;

        BattleUIService?.CheckAndZoom();
    }

    public void OnLateUpdateTick()
    {
        if (ModServiceConfigurator.Provider == null || ModServiceConfigurator.CurrentScope == null || ConfigAccessor == null)
            return;

        TextDataService?.ApplyPendingConstantsRefresh();
        BattleUIService?.ApplyPendingConfigChanges();

        // Hotkey polling: only when in-level and native zoom-in completed
        if (ConfigAccessor.Main.EnableUiToggleHotkey &&
            (GameStatsService?.IsPlayerPlaying ?? false) &&
            (BattleUIService?.NativeZoomInCompleted ?? false))
        {
            if (HotkeyService?.CheckToggleTriggered() ?? false)
                BattleUIService?.SetDesiredUiVisible(!(BattleUIService?.DesiredUiVisible ?? true));
        }

        if (!(GameStatsService?.IsPlayerPlaying ?? false))
            return;

        var now = NowMs();

        var dataInterval = Math.Max(0, ConfigAccessor.Advanced.DataRefreshIntervalMs);
        var textInterval = ConfigAccessor.Advanced.TextRefreshIntervalMs <= 0 ? dataInterval : ConfigAccessor.Advanced.TextRefreshIntervalMs;
        if (textInterval < dataInterval) textInterval = dataInterval;

        if (now - _lastDataMs >= dataInterval)
        {
            _lastDataMs = now;
            GameStatsService.UpdateCurrentStats();
            _dataTriggerCount++;
        }

        if (now - _lastTextMs >= textInterval)
        {
            _lastTextMs = now;
            TextObjectService?.UpdateAllText();
            _textTriggerCount++;
        }

        if (ConfigAccessor.Advanced.DisplayNoteDebuggingData)
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

        Logger.Info($"Trigger counts: data({_dataTriggerCount}) text({_textTriggerCount})");

        _dataTriggerCount = 0;
        _textTriggerCount = 0;
    }

    [UsedImplicitly] [Inject] public IConfigAccessor ConfigAccessor { get; set; }
    [UsedImplicitly] [Inject] public IBattleUIService BattleUIService { get; set; }
    [UsedImplicitly] [Inject] public IGameStatsService GameStatsService { get; set; }
    [UsedImplicitly] [Inject] public ITextObjectService TextObjectService { get; set; }
    [UsedImplicitly] [Inject] public ITextDataService TextDataService { get; set; }
    [UsedImplicitly] [Inject] public IHotkeyService HotkeyService { get; set; }
    [UsedImplicitly] [Inject] public ILogger<RefreshScheduler> Logger { get; set; }
}