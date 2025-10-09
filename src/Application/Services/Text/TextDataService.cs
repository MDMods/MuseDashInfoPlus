using System.Globalization;
using System.Text;
using JetBrains.Annotations;
using MDIP.Application.Services.Configuration;
using MDIP.Application.Services.Stats;
using MDIP.Domain.Configs;
using MDIP.Utils;

namespace MDIP.Application.Services.Text;

public class TextDataService : ITextDataService
{
    private static readonly char[] TrimChars = ['|', '\\', '-', '/', '~', '_', '=', '+'];

    private readonly Dictionary<string, string> _cachedValues = new();
    private readonly Dictionary<string, string> _formattedTexts = new();
    private bool _callbacksRegistered;

    public void EnsureCallbacksRegistered()
    {
        if (_callbacksRegistered)
            return;

        TryRegisterCallbacks();
    }

    public void UpdateConstants()
    {
        EnsureCallbacksRegistered();

        var main = ConfigAccessor.Main;
        UpdateCachedValue("{pbScore}", GameStatsService.History.Score.ToString());
        UpdateCachedValue("{pbAcc}", $"{GameStatsService.History.Accuracy}%");
        UpdateCachedValue("{total}", ((int)GameStatsService.AccuracyCalculationTotal).ToString());
        UpdateCachedValue("{song}", GameUtils.MusicName.TruncateByWidth(45));
        UpdateCachedValue("{diff}", GameUtils.GetDifficultyLabel(GameStatsService, main));
        UpdateCachedValue("{level}", GameUtils.MusicLevel);
        UpdateCachedValue("{author}", GameUtils.MusicAuthor);
        UpdateCachedValue("{pbGreat}", GameStatsService.History.Great.ToString());
        UpdateCachedValue("{pbMissOther}", GameStatsService.History.MissOther.ToString());
        UpdateCachedValue("{pbMissCollectible}", GameStatsService.History.MissCollectible.ToString());
        UpdateCachedValue("{pbEarly}", GameStatsService.History.Early.ToString());
        UpdateCachedValue("{pbLate}", GameStatsService.History.Late.ToString());

        if (!GameStatsService.History.HasStats)
        {
            UpdateCachedValue("{pbStats}", main.StatsGapTextWhenNoPersonalBest);
            UpdateCachedValue("{pbStatsGap}", main.StatsGapTextWhenNoPersonalBest);
        }

        if (!GameStatsService.IsFirstTry)
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
        UpdateCachedValue("{time}", Helper.SafeFormatDateTime(DateTime.Now, main.TimeDisplayFormat, CultureInfo.CurrentCulture.Name));

        if (GameStatsService.History.HasStats)
        {
            UpdateCachedValue("{pbStats}", GameStatsService.FormatPersonalBestStats());
            UpdateCachedValue("{pbStatsGap}", GameStatsService.FormatPersonalBestStatsGap());
        }

        if (GameStatsService.IsFirstTry)
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

        var result = new StringBuilder(originalText);
        foreach (var pair in _cachedValues.Where(pair => result.ToString().Contains(pair.Key)))
            result.Replace(pair.Key, pair.Value);

        var finalText = result.ToString().Trim().Trim(TrimChars).Trim();
        _formattedTexts[originalText] = finalText;
        return finalText;
    }

    private void TryRegisterCallbacks()
    {
        if (_callbacksRegistered)
            return;

        try
        {
            ConfigService.RegisterUpdateCallback<MainConfigs>(nameof(MainConfigs), _ => ClearCaches());
            ConfigService.RegisterUpdateCallback<AdvancedConfigs>(nameof(AdvancedConfigs), _ => ClearCaches());
            ConfigService.RegisterUpdateCallback<TextFieldLowerLeftConfigs>(nameof(TextFieldLowerLeftConfigs), _ => ClearCaches());
            ConfigService.RegisterUpdateCallback<TextFieldLowerRightConfigs>(nameof(TextFieldLowerRightConfigs), _ => ClearCaches());
            ConfigService.RegisterUpdateCallback<TextFieldScoreBelowConfigs>(nameof(TextFieldScoreBelowConfigs), _ => ClearCaches());
            ConfigService.RegisterUpdateCallback<TextFieldScoreRightConfigs>(nameof(TextFieldScoreRightConfigs), _ => ClearCaches());
            ConfigService.RegisterUpdateCallback<TextFieldUpperLeftConfigs>(nameof(TextFieldUpperLeftConfigs), _ => ClearCaches());
            ConfigService.RegisterUpdateCallback<TextFieldUpperRightConfigs>(nameof(TextFieldUpperRightConfigs), _ => ClearCaches());

            _callbacksRegistered = true;
        }
        catch (InvalidOperationException)
        {
            // Keep _callbacksRegistered false, wait for next invoke
        }
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

    [UsedImplicitly] public IConfigAccessor ConfigAccessor { get; set; }
    [UsedImplicitly] public IConfigService ConfigService { get; set; }
    [UsedImplicitly] public IGameStatsService GameStatsService { get; set; }
}