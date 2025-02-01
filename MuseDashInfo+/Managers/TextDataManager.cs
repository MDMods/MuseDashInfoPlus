namespace MDIP.Managers;

public static class TextDataManager
{
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
		UpdateCachedValue("{overview}", GameStatsManager.FormatOverview());
		UpdateCachedValue("{stats}", GameStatsManager.FormatStats());
		UpdateCachedValue("{gap}", GameStatsManager.FormatScoreGap());
		UpdateCachedValue("{hit}", ((int)GameStatsManager.AccuracyCounted).ToString());
	}

	private static void InvalidateFormattedTexts()
		=> FormattedTexts.Clear();

	public static string GetFormattedText(string originalText)
	{
		if (string.IsNullOrWhiteSpace(originalText)) return string.Empty;

		if (FormattedTexts.TryGetValue(originalText, out var formatted))
			return formatted;

		if (!ContainsAnyPlaceholder(originalText))
			return originalText;

		var result = originalText;
		foreach (var pair in CachedValues.Where(pair => result.Contains(pair.Key)))
			result = result.Replace(pair.Key, pair.Value);

		var trimChars = new[] { '|', '\\', '-', '/', '~', '_', '=', '+' };
		result = result.Trim().Trim(trimChars).Trim();

		FormattedTexts[originalText] = result;
		return result;
	}

	private static bool ContainsAnyPlaceholder(string text)
		=> text.Contains('{') && text.Contains('}');
}