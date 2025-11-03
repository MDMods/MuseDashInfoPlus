using System.Globalization;
using Il2CppAssets.Scripts.Database;
using JetBrains.Annotations;
using MDIP.Application.DependencyInjection;
using MDIP.Application.Services.Global.Configuration;
using MDIP.Application.Services.Global.Logging;
using MDIP.Application.Services.Global.RuntimeData;
using MDIP.Application.Services.Scoped.Stats;
using MDIP.Application.Services.Scoped.UI;
using MDIP.Core.Domain.Configs;
using MDIP.Core.Utilities;

namespace MDIP.Application.Services.Global.Text;

public class TextDataService : ITextDataService
{
    private static readonly char[] TrimChars = ['|', '\\', '-', '/', '~', '_', '=', '+'];

    private int _pendingCacheClear;
    private int _pendingConstantsRefresh;
    private int _globalCallbacksState;

    private readonly Dictionary<string, string> _cachedValues = new();
    private readonly Dictionary<string, string> _formattedTexts = new();
    private bool _callbacksRegistered;

    public void UpdateConstants()
    {
        EnsureCallbacksRegistered();

        var main = ConfigAccessor.Main;
        UpdateCachedValue("{pbScore}", GameStatsService.History.Score.ToString());
        UpdateCachedValue("{pbAcc}", $"{GameStatsService.History.Accuracy}%");
        UpdateCachedValue("{total}", ((int)GameStatsService.AccuracyCalculationTotal).ToString());
        UpdateCachedValue("{song}", MusicInfoUtils.CurMusicName.TruncateByWidth(45));
        UpdateCachedValue("{level}", MusicInfoUtils.GetDifficultyLabel(ConfigAccessor.Main));
        UpdateCachedValue("{diff}", MusicInfoUtils.CurMusicDiffStr);
        UpdateCachedValue("{albumName}", MusicInfoUtils.CurAlbumName);
        UpdateCachedValue("{author}", MusicInfoUtils.CurMusicAuthor);
        UpdateCachedValue("{levelDesigner}", MusicInfoUtils.CurMusicLevelDesigner);
        UpdateCachedValue("{bpm}", MusicInfoUtils.CurMusicBpm);
        UpdateCachedValue("{pbGreat}", GameStatsService.History.Great.ToString());
        UpdateCachedValue("{pbMissOther}", GameStatsService.History.MissOther.ToString());
        UpdateCachedValue("{pbMissCollectible}", GameStatsService.History.MissCollectible.ToString());
        UpdateCachedValue("{pbEarly}", GameStatsService.History.Early.ToString());
        UpdateCachedValue("{pbLate}", GameStatsService.History.Late.ToString());
        UpdateCachedValue("{userLanguage}", DataHelper.userLanguage);
        UpdateCachedValue("{offset}", DataHelper.offset.ToString());
        UpdateCachedValue("{playerName}", DataHelper.nickname);
        UpdateCachedValue("{playerLevel}", DataHelper.Level.ToString());
        UpdateCachedValue("{bgmVolume}", DataHelper.bgmVolume.ToString("F2"));
        UpdateCachedValue("{hitSfxVolume}", DataHelper.hitSfxVolume.ToString("F2"));
        UpdateCachedValue("{voiceVolume}", DataHelper.voiceVolume.ToString("F2"));
        UpdateCachedValue("{elfin}", MusicInfoUtils.CurElfin);
        UpdateCachedValue("{character}", MusicInfoUtils.CurGirl);

        if (!GameStatsService.History.HasStats)
        {
            UpdateCachedValue("{pbStats}", main.StatsGapTextWhenNoPersonalBest);
            UpdateCachedValue("{pbStatsGap}", main.StatsGapTextWhenNoPersonalBest);
        }

        if (!RuntimeDataStore.IsFirstTry(MusicInfoUtils.CurMusicHash))
            return;

        UpdateCachedValue("{scoreGap}", main.ScoreGapTextWhenNoPersonalBest);
        UpdateCachedValue("{accGap}", main.AccuracyGapTextWhenNoPersonalBest);
    }

    public void UpdateVariables()
    {
        EnsureCallbacksRegistered();

        var main = ConfigAccessor.Main;
        UpdateCachedValue("{acc}", GameStatsService.FormatAccuracy());
        UpdateCachedValue("{overview}", GameStatsService.FormatOverview());
        UpdateCachedValue("{stats}", GameStatsService.FormatStats());
        UpdateCachedValue("{hit}", ((int)(GameStatsService.AccuracyCalculationCounted + GameStatsService.Current.Great / 2f)).ToString());
        UpdateCachedValue("{skySpeed}", GameStatsService.CurrentSkySpeed.ToString());
        UpdateCachedValue("{groundSpeed}", GameStatsService.CurrentGroundSpeed.ToString());
        UpdateCachedValue("{rank}", GameStatsService.FormatRank());
        UpdateCachedValue("{time}", DateTime.Now.SafeFormatDateTime(main.TimeDisplayFormat, CultureInfo.CurrentCulture.Name));

        if (GameStatsService.History.HasStats)
        {
            UpdateCachedValue("{pbStats}", GameStatsService.FormatPersonalBestStats());
            UpdateCachedValue("{pbStatsGap}", GameStatsService.FormatPersonalBestStatsGap());
        }

        if (RuntimeDataStore.IsFirstTry(MusicInfoUtils.CurMusicHash))
            return;

        UpdateCachedValue("{scoreGap}", GameStatsService.FormatScoreGap());
        UpdateCachedValue("{accGap}", GameStatsService.FormatAccuracyGap());
    }

    public string GetFormattedText(string originalText)
    {
        if (string.IsNullOrEmpty(originalText) || !ContainsAnyPlaceholder(originalText))
            return originalText;

        if (_formattedTexts.TryGetValue(originalText, out var formatted))
            return formatted;

        var text = originalText;
        foreach (var (key, value) in _cachedValues)
        {
            if (text.Contains(key, StringComparison.OrdinalIgnoreCase))
                text = text.Replace(key, value ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        var finalText = text.Trim().Trim(TrimChars).Trim();
        _formattedTexts[originalText] = finalText;
        return finalText;
    }

    public void EnsureCallbacksRegistered()
    {
        if (_callbacksRegistered)
            return;

        TryRegisterCallbacks();
    }

    public void ApplyPendingConstantsRefresh()
    {
        if (Interlocked.Exchange(ref _pendingCacheClear, 0) == 1)
            ClearCaches();

        if (Interlocked.Exchange(ref _pendingConstantsRefresh, 0) == 1)
            UpdateConstants();
    }

    private void TryRegisterCallbacks()
    {
        if (_callbacksRegistered)
            return;

        if (Volatile.Read(ref _globalCallbacksState) == 1)
        {
            _callbacksRegistered = true;
            return;
        }

        if (ConfigService == null)
            return;

        if (Interlocked.CompareExchange(ref _globalCallbacksState, -1, 0) != 0)
            return;

        try
        {
            ConfigService.RegisterUpdateCallback<MainConfigs>(nameof(MainConfigs), HandleConfigChanged);
            ConfigService.RegisterUpdateCallback<AdvancedConfigs>(nameof(AdvancedConfigs), HandleConfigChanged);
            ConfigService.RegisterUpdateCallback<TextFieldLowerLeftConfigs>(nameof(TextFieldLowerLeftConfigs), HandleConfigChanged);
            ConfigService.RegisterUpdateCallback<TextFieldLowerRightConfigs>(nameof(TextFieldLowerRightConfigs), HandleConfigChanged);
            ConfigService.RegisterUpdateCallback<TextFieldScoreBelowConfigs>(nameof(TextFieldScoreBelowConfigs), HandleConfigChanged);
            ConfigService.RegisterUpdateCallback<TextFieldScoreRightConfigs>(nameof(TextFieldScoreRightConfigs), HandleConfigChanged);
            ConfigService.RegisterUpdateCallback<TextFieldUpperLeftConfigs>(nameof(TextFieldUpperLeftConfigs), HandleConfigChanged);
            ConfigService.RegisterUpdateCallback<TextFieldUpperRightConfigs>(nameof(TextFieldUpperRightConfigs), HandleConfigChanged);

            Volatile.Write(ref _globalCallbacksState, 1);
            _callbacksRegistered = true;
        }
        catch (Exception)
        {
            Volatile.Write(ref _globalCallbacksState, 0);
        }
    }

    private void HandleConfigChanged<T>(T _) where T : class
        => HandleConfigChangedCore();

    private void HandleConfigChangedCore()
    {
        Interlocked.Exchange(ref _pendingCacheClear, 1);
        Interlocked.Exchange(ref _pendingConstantsRefresh, 1);

        var battleUiService = ModServiceConfigurator.Provider?.GetService(typeof(IBattleUIService)) as IBattleUIService;
        battleUiService?.QueueApplyConfigChanges();
    }

    private void UpdateCachedValue(string key, string value)
    {
        if (_cachedValues.TryGetValue(key, out var cached) && cached == value)
            return;

        _cachedValues[key] = value ?? string.Empty;
        _formattedTexts.Clear();
    }

    private void ClearCaches()
    {
        _cachedValues.Clear();
        _formattedTexts.Clear();
    }

    private static bool ContainsAnyPlaceholder(string text)
        => !string.IsNullOrEmpty(text) && text.Contains('{') && text.Contains('}');

    [UsedImplicitly] [Inject] public IConfigAccessor ConfigAccessor { get; set; }
    [UsedImplicitly] [Inject] public IConfigService ConfigService { get; set; }
    [UsedImplicitly] [Inject] public IGameStatsService GameStatsService { get; set; }
    [UsedImplicitly] [Inject] public IRuntimeDataStore RuntimeDataStore { get; set; }
    [UsedImplicitly] [Inject] public ILogger<TextDataService> Logger { get; set; }
}