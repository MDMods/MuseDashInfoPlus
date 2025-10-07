using System.Globalization;
using System.Text;

namespace MDIP.Services;

public class TextDataService : ITextDataService
{
    private static readonly char[] TrimChars = { '|', '\\', '-', '/', '~', '_', '=', '+' };
    private readonly Dictionary<string, string> _cachedValues = new();
    private readonly Dictionary<string, string> _formattedTexts = new();

    public void UpdateConstants()
    {
        UpdateCachedValue("{pbScore}", GameStatsManager.History.Score.ToString());
        UpdateCachedValue("{pbAcc}", $"{GameStatsManager.History.Accuracy}%");
        UpdateCachedValue("{total}", ((int)GameStatsManager.AccuracyCalculationTotal).ToString());
        UpdateCachedValue("{song}", GameUtils.MusicName.TruncateByWidth(45));
        UpdateCachedValue("{diff}", GameUtils.MusicDiffStr);
        UpdateCachedValue("{level}", GameUtils.MusicLevel);
        UpdateCachedValue("{author}", GameUtils.MusicAuthor);
        UpdateCachedValue("{pbGreat}", GameStatsManager.History.Great.ToString());
        UpdateCachedValue("{pbMissOther}", GameStatsManager.History.MissOther.ToString());
        UpdateCachedValue("{pbMissCollectible}", GameStatsManager.History.MissCollectible.ToString());
        UpdateCachedValue("{pbEarly}", GameStatsManager.History.Early.ToString());
        UpdateCachedValue("{pbLate}", GameStatsManager.History.Late.ToString());

        if (!GameStatsManager.History.HasStats)
        {
            UpdateCachedValue("{pbStats}", Configs.Main.StatsGapTextWhenNoPersonalBest);
            UpdateCachedValue("{pbStatsGap}", Configs.Main.StatsGapTextWhenNoPersonalBest);
        }

        if (!GameStatsManager.IsFirstTry)
            return;

        UpdateCachedValue("{scoreGap}", Configs.Main.ScoreGapTextWhenNoPersonalBest);
        UpdateCachedValue("{accGap}", Configs.Main.AccuracyGapTextWhenNoPersonalBest);
    }

    public void UpdateVariables()
    {
        UpdateCachedValue("{acc}", GameStatsManager.FormatAccuracy());
        UpdateCachedValue("{overview}", GameStatsManager.FormatOverview());
        UpdateCachedValue("{stats}", GameStatsManager.FormatStats());
        UpdateCachedValue("{hit}", ((int)(GameStatsManager.AccuracyCalculationCounted + GameStatsManager.Current.Great / 2f)).ToString());
        UpdateCachedValue("{skySpeed}", GameStatsManager.CurrentSkySpeed.ToString());
        UpdateCachedValue("{groundSpeed}", GameStatsManager.CurrentGroundSpeed.ToString());
        UpdateCachedValue("{rank}", GameStatsManager.FormatRank());
        UpdateCachedValue("{time}", Helper.SafeFormatDateTime(DateTime.Now, Configs.Main.TimeDisplayFormat, CultureInfo.CurrentCulture.Name));

        if (GameStatsManager.History.HasStats)
        {
            UpdateCachedValue("{pbStats}", GameStatsManager.FormatPersonalBestStats());
            UpdateCachedValue("{pbStatsGap}", GameStatsManager.FormatPersonalBestStatsGap());
        }

        if (GameStatsManager.IsFirstTry)
            return;

        UpdateCachedValue("{scoreGap}", GameStatsManager.FormatScoreGap());
        UpdateCachedValue("{accGap}", GameStatsManager.FormatAccuracyGap());
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

    private void UpdateCachedValue(string key, string value)
    {
        if (_cachedValues.TryGetValue(key, out var cached) && cached == value)
            return;

        _cachedValues[key] = value ?? string.Empty;
        _formattedTexts.Clear();
    }

    private static bool ContainsAnyPlaceholder(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;
        return text.Contains('{') && text.Contains('}');
    }
}