using System.Collections.Generic;

using MDIP.Utils;

namespace MDIP.Managers;

public static class TextDataManager
{
    private static Dictionary<string, string> _cachedValues = new();
    private static Dictionary<string, string> _formattedTexts = new();

    private static void UpdateCachedValue(string key, string value)
    {
        if (!_cachedValues.ContainsKey(key) || _cachedValues[key] != value)
        {
            _cachedValues[key] = value;
            InvalidateFormattedTexts();
        }
    }

    public static void UpdateConstants()
    {
        UpdateCachedValue("{hiScore}", GameStatsManager.SavedHighScore.ToString());
        UpdateCachedValue("{total}", ((int)GameStatsManager.AccuracyTotal).ToString());
        UpdateCachedValue("{song}", GameUtils.MusicName.TruncateByWidth(45));
        UpdateCachedValue("{diff}", GameUtils.MusicDiffStr);
        UpdateCachedValue("{level}", GameUtils.MusicLevel);
        UpdateCachedValue("{author}", GameUtils.MusicAuthor);
    }

    public static void UpdateValues()
    {
        UpdateCachedValue("{acc}", GameStatsManager.FormatAccuracy());
        UpdateCachedValue("{overview}", GameStatsManager.FormatAccuracy());
        UpdateCachedValue("{stats}", GameStatsManager.FormatStats());
        UpdateCachedValue("{gap}", GameStatsManager.FormatScoreGap());
        UpdateCachedValue("{hit}", ((int)GameStatsManager.AccuracyCounted).ToString());
        UpdateCachedValue("{miss}", (GameStatsManager.MissCountHitable + GameStatsManager.Miss.Block).ToString());
        UpdateCachedValue("{missCollectable}", GameStatsManager.MissCountCollectable.ToString());
        UpdateCachedValue("{great}", GameStatsManager.Current.Great.ToString());
        UpdateCachedValue("{early}", GameStatsManager.Current.Early.ToString());
        UpdateCachedValue("{late}", GameStatsManager.Current.Late.ToString());
    }

    private static void InvalidateFormattedTexts()
    {
        _formattedTexts.Clear();
    }

    public static string GetFormattedText(string originalText)
    {
        if (string.IsNullOrWhiteSpace(originalText)) return string.Empty;

        string cacheKey = originalText;
        if (_formattedTexts.TryGetValue(cacheKey, out string formatted))
            return formatted;

        if (!ContainsAnyPlaceholder(originalText))
            return originalText;

        string result = originalText;
        foreach (var pair in _cachedValues)
        {
            if (result.Contains(pair.Key))
                result = result.Replace(pair.Key, pair.Value);
        }

        var trimChars = new[] { '|', '\\', '-', '/', '~', '_', '=', '+' };
        result = result.Trim().Trim(trimChars).Trim();

        _formattedTexts[cacheKey] = result;
        return result;
    }

    private static bool ContainsAnyPlaceholder(string text)
    {
        return text.Contains('{') && text.Contains('}');
    }
}
