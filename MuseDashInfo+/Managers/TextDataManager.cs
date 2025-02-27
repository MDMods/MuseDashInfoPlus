using System.Text;
using static MDIP.Managers.GameStatsManager;

namespace MDIP.Managers;

public static class TextDataManager
{
    private static readonly char[] TrimChars = ['|', '\\', '-', '/', '~', '_', '=', '+'];
    private static Dictionary<string, string> CachedValues { get; } = new();
    private static Dictionary<string, string> FormattedTexts { get; } = new();

    private static void UpdateCachedValue(string key, string value)
    {
        if (CachedValues.ContainsKey(key) && CachedValues[key] == value)
            return;

        CachedValues[key] = value;
        InvalidateFormattedTexts();
    }

    public static void UpdateConstants()
    {
        UpdateCachedValue("{pbScore}", History.Score.ToString());
        UpdateCachedValue("{pbAcc}", $"{History.Accuracy}%");
        UpdateCachedValue("{total}", ((int)AccuracyCalculationTotal).ToString());
        UpdateCachedValue("{song}", GameUtils.MusicName.TruncateByWidth(45));
        UpdateCachedValue("{diff}", GameUtils.MusicDiffStr);
        UpdateCachedValue("{level}", GameUtils.MusicLevel);
        UpdateCachedValue("{author}", GameUtils.MusicAuthor);
        UpdateCachedValue("{pbGreat}", History.Great.ToString());
        UpdateCachedValue("{pbMissOther}", History.MissOther.ToString());
        UpdateCachedValue("{pbMissCollectible}", History.MissCollectible.ToString());
        UpdateCachedValue("{pbEarly}", History.Early.ToString());
        UpdateCachedValue("{pbLate}", History.Late.ToString());

        if (!History.HasStats)
        {
            UpdateCachedValue("{pbStats}", Configs.Main.StatsGapTextWhenNoPersonalBest);
            UpdateCachedValue("{pbStatsGap}", Configs.Main.StatsGapTextWhenNoPersonalBest);
        }

        if (!IsFirstTry) return;
        UpdateCachedValue("{scoreGap}", Configs.Main.ScoreGapTextWhenNoPersonalBest);
        UpdateCachedValue("{accGap}", Configs.Main.AccuracyGapTextWhenNoPersonalBest);
    }

    public static void UpdateVariables()
    {
        UpdateCachedValue("{acc}", FormatAccuracy());
        UpdateCachedValue("{overview}", FormatOverview());
        UpdateCachedValue("{stats}", FormatStats());
        UpdateCachedValue("{hit}", ((int)(AccuracyCalculationCounted + Current.Great / 2f)).ToString());
        UpdateCachedValue("{skySpeed}", CurrentSkySpeed.ToString());
        UpdateCachedValue("{groundSpeed}", CurrentGroundSpeed.ToString());
        UpdateCachedValue("{rank}", FormatRank());

        if (History.HasStats)
        {
            UpdateCachedValue("{pbStats}", FormatPersonalBestStats());
            UpdateCachedValue("{pbStatsGap}", FormatPersonalBestStatsGap());
        }

        if (IsFirstTry) return;
        UpdateCachedValue("{scoreGap}", FormatScoreGap());
        UpdateCachedValue("{accGap}", FormatAccuracyGap());
    }

    private static void InvalidateFormattedTexts()
        => FormattedTexts.Clear();

    public static string GetFormattedText(string originalText)
    {
        if (!ContainsAnyPlaceholder(originalText))
            return originalText;

        if (FormattedTexts.TryGetValue(originalText, out var formatted))
            return formatted;

        var sb = new StringBuilder(originalText);
        foreach (var pair in CachedValues.Where(pair => sb.ToString().Contains(pair.Key)))
            sb.Replace(pair.Key, pair.Value);

        var result = sb.ToString().Trim().Trim(TrimChars).Trim();

        FormattedTexts[originalText] = result;
        return result;
    }

    private static bool ContainsAnyPlaceholder(string text)
        => text.Contains('{') && text.Contains('}');
}