using System.Globalization;
using Il2CppAssets.Scripts.Database;
using MDIP.Battle;
using MDIP.Core.Utilities;

namespace MDIP.Globals;

// Resolves the {placeholder} tokens used in the text-field configs into display strings, caching the
// formatted results. Constants (song/pb/level/…) are computed once per battle; variables (acc/rank/…)
// every refresh. The current battle's stats are passed in by the caller — TextData holds no battle
// state of its own beyond the value caches.
internal static class TextData
{
    private static readonly char[] TrimChars = ['|', '\\', '-', '/', '~', '_', '=', '+'];

    private static int _pendingCacheClear;
    private static int _pendingConstantsRefresh;

    private static readonly Dictionary<string, string> CachedValues = new();
    private static readonly Dictionary<string, string> FormattedTexts = new();

    public static void UpdateConstants(GameStats stats)
    {
        if (stats == null)
            return;

        var main = Config.Main;
        UpdateCachedValue("{pbScore}", stats.History.Score.ToString());
        UpdateCachedValue("{pbAcc}", $"{stats.History.Accuracy}%");
        UpdateCachedValue("{total}", ((int)stats.AccuracyCalculationTotal).ToString());
        UpdateCachedValue("{song}", MusicInfoUtils.CurMusicName.TruncateByWidth(45));
        UpdateCachedValue("{level}", MusicInfoUtils.GetDifficultyLabel(main));
        UpdateCachedValue("{diff}", MusicInfoUtils.CurMusicDiffStr);
        UpdateCachedValue("{albumName}", MusicInfoUtils.CurAlbumName);
        UpdateCachedValue("{author}", MusicInfoUtils.CurMusicAuthor);
        UpdateCachedValue("{levelDesigner}", MusicInfoUtils.CurMusicLevelDesigner);
        UpdateCachedValue("{bpm}", MusicInfoUtils.CurMusicBpm);
        UpdateCachedValue("{pbGreat}", stats.History.Great.ToString());
        UpdateCachedValue("{pbMissOther}", stats.History.MissOther.ToString());
        UpdateCachedValue("{pbMissCollectible}", stats.History.MissCollectible.ToString());
        UpdateCachedValue("{pbEarly}", stats.History.Early.ToString());
        UpdateCachedValue("{pbLate}", stats.History.Late.ToString());
        UpdateCachedValue("{userLanguage}", DataHelper.userLanguage);
        UpdateCachedValue("{offset}", DataHelper.offset.ToString());
        UpdateCachedValue("{playerName}", DataHelper.nickname);
        UpdateCachedValue("{playerLevel}", DataHelper.Level.ToString());
        UpdateCachedValue("{bgmVolume}", DataHelper.bgmVolume.ToString("F2"));
        UpdateCachedValue("{hitSfxVolume}", DataHelper.hitSfxVolume.ToString("F2"));
        UpdateCachedValue("{voiceVolume}", DataHelper.voiceVolume.ToString("F2"));
        UpdateCachedValue("{elfin}", MusicInfoUtils.CurElfin);
        UpdateCachedValue("{character}", MusicInfoUtils.CurGirl);

        if (!stats.History.HasStats)
        {
            UpdateCachedValue("{pbStats}", main.StatsGapTextWhenNoPersonalBest);
            UpdateCachedValue("{pbStatsGap}", main.StatsGapTextWhenNoPersonalBest);
        }

        if (!RuntimeData.IsFirstTry(MusicInfoUtils.CurMusicHash))
            return;

        UpdateCachedValue("{scoreGap}", main.ScoreGapTextWhenNoPersonalBest);
        UpdateCachedValue("{accGap}", main.AccuracyGapTextWhenNoPersonalBest);
    }

    public static void UpdateVariables(GameStats stats)
    {
        if (stats == null)
            return;

        var main = Config.Main;
        UpdateCachedValue("{acc}", stats.FormatAccuracy());
        UpdateCachedValue("{overview}", stats.FormatOverview());
        UpdateCachedValue("{stats}", stats.FormatStats());
        UpdateCachedValue("{hit}", ((int)(stats.AccuracyCalculationCounted + stats.Current.Great / 2f)).ToString());
        UpdateCachedValue("{skySpeed}", stats.CurrentSkySpeed.ToString());
        UpdateCachedValue("{groundSpeed}", stats.CurrentGroundSpeed.ToString());
        UpdateCachedValue("{rank}", stats.FormatRank());
        UpdateCachedValue("{time}", DateTime.Now.SafeFormatDateTime(main.TimeDisplayFormat, CultureInfo.CurrentCulture.Name));

        if (stats.History.HasStats)
        {
            UpdateCachedValue("{pbStats}", stats.FormatPersonalBestStats());
            UpdateCachedValue("{pbStatsGap}", stats.FormatPersonalBestStatsGap());
        }

        if (RuntimeData.IsFirstTry(MusicInfoUtils.CurMusicHash))
            return;

        UpdateCachedValue("{scoreGap}", stats.FormatScoreGap());
        UpdateCachedValue("{accGap}", stats.FormatAccuracyGap());
    }

    public static string GetFormattedText(string originalText)
    {
        if (string.IsNullOrEmpty(originalText) || !ContainsAnyPlaceholder(originalText))
            return originalText;

        if (FormattedTexts.TryGetValue(originalText, out var formatted))
            return formatted;

        var text = originalText;
        foreach (var (key, value) in CachedValues)
        {
            if (text.Contains(key, StringComparison.OrdinalIgnoreCase))
                text = text.Replace(key, value ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        var finalText = text.Trim().Trim(TrimChars).Trim();
        FormattedTexts[originalText] = finalText;
        return finalText;
    }

    // Queued by the config-change handler (wired once at startup): the next refresh tick clears the
    // caches and recomputes constants, so a mid-battle config edit takes effect live.
    public static void QueueRefresh()
    {
        Interlocked.Exchange(ref _pendingCacheClear, 1);
        Interlocked.Exchange(ref _pendingConstantsRefresh, 1);
    }

    public static void ApplyPendingConstantsRefresh(GameStats stats)
    {
        if (Interlocked.Exchange(ref _pendingCacheClear, 0) == 1)
            ClearCaches();

        if (Interlocked.Exchange(ref _pendingConstantsRefresh, 0) == 1)
            UpdateConstants(stats);
    }

    public static void ClearCaches()
    {
        CachedValues.Clear();
        FormattedTexts.Clear();
    }

    private static void UpdateCachedValue(string key, string value)
    {
        if (CachedValues.TryGetValue(key, out var cached) && cached == value)
            return;

        CachedValues[key] = value ?? string.Empty;
        FormattedTexts.Clear();
    }

    private static bool ContainsAnyPlaceholder(string text)
        => !string.IsNullOrEmpty(text) && text.Contains('{') && text.Contains('}');
}
