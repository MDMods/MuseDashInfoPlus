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

    public static void UpdateValues()
    {
        UpdateCachedValue("{acc}", GameStatsManager.FormatAccuracy());
        UpdateCachedValue("{stats}", GameStatsManager.FormatStats());
        UpdateCachedValue("{highest}", GameStatsManager.SavedHighScore.ToString());
        UpdateCachedValue("{gap}", GameStatsManager.FormatScoreGap());
        UpdateCachedValue("{total}", ((int)GameStatsManager.AccuracyTotal).ToString());
        UpdateCachedValue("{hit}", ((int)GameStatsManager.AccuracyCounted).ToString());
        UpdateCachedValue("{song}", GameUtils.MusicName);
        UpdateCachedValue("{diff}", GameUtils.MusicDiffStr);
        UpdateCachedValue("{level}", GameUtils.MusicLevel);
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
