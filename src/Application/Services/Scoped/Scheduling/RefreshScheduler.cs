using MDIP.Application.DependencyInjection;
using MDIP.Application.Services.Global.Configuration;
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

    private readonly HashSet<string> _missingLogged = [];

    public void OnFixedUpdateTick()
    {
        if (ModServiceConfigurator.Provider == null || ModServiceConfigurator.CurrentScope == null)
            return;

        var provider = ModServiceConfigurator.Provider;
        var battle = provider.GetService(typeof(IBattleUIService)) as IBattleUIService;
        if (battle == null)
        {
            LogMissingOnce(nameof(IBattleUIService));
            return;
        }

        battle.CheckAndZoom();
    }

    public void OnLateUpdateTick()
    {
        if (ModServiceConfigurator.Provider == null || ModServiceConfigurator.CurrentScope == null)
            return;

        var provider = ModServiceConfigurator.Provider;
        var config = provider.GetService(typeof(IConfigAccessor)) as IConfigAccessor;
        var stats = provider.GetService(typeof(IGameStatsService)) as IGameStatsService;
        var textObj = provider.GetService(typeof(ITextObjectService)) as ITextObjectService;

        if (config == null)
        {
            LogMissingOnce(nameof(IConfigAccessor));
            return;
        }
        if (stats == null)
        {
            LogMissingOnce(nameof(IGameStatsService));
            return;
        }
        if (textObj == null)
        {
            LogMissingOnce(nameof(ITextObjectService));
            return;
        }

        if (!stats.IsInGame)
            return;

        var now = NowMs();

        var dataInterval = Math.Max(0, config.Advanced.DataRefreshIntervalMs);
        var textInterval = config.Advanced.TextRefreshIntervalMs <= 0 ? dataInterval : config.Advanced.TextRefreshIntervalMs;
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
            textObj.UpdateAllText();
            _textTriggerCount++;
        }

        if (config.Advanced.DisplayNoteDebuggingData)
            ReportPerSecond();
    }

    public void Reset()
    {
        _lastDataMs = 0;
        _lastTextMs = 0;
        _dataTriggerCount = 0;
        _textTriggerCount = 0;
        _lastReportSecond = -1;
    }

    private static long NowMs() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    private void ReportPerSecond()
    {
        var nowSec = DateTime.Now.Second;
        if (_lastReportSecond == nowSec)
            return;
        _lastReportSecond = nowSec;

        var logger = GetLogger();
        logger?.Info($"[Scheduler] data:{_dataTriggerCount} text:{_textTriggerCount}");
        _dataTriggerCount = 0;
        _textTriggerCount = 0;
    }

    private void LogMissingOnce(string name)
    {
        if (_missingLogged.Add(name))
            GetLogger()?.Warn($"Service '{name}' not ready; scheduler tick skipped.");
    }

    private static ILogger<RefreshScheduler> GetLogger()
    {
        var provider = ModServiceConfigurator.Provider;
        return provider?.GetService(typeof(ILogger<RefreshScheduler>)) as ILogger<RefreshScheduler>;
    }
}