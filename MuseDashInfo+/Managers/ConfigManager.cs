using MelonLoader;

using MuseDashInfoPlus.Utils;

namespace MuseDashInfoPlus.Manager;

public static class ConfigManager
{
    private static MelonPreferences_Category _category;
    // Upper right corner
    private static MelonPreferences_Entry<bool> _displaySongName;
    private static MelonPreferences_Entry<bool> _displaySongDifficulty;
    private static MelonPreferences_Entry<string> _customSongDifficultyFormat;
    // Upper left corner
    private static MelonPreferences_Entry<bool> _displayHitCounts;
    private static MelonPreferences_Entry<string> _customHitCountsFormat;
    private static MelonPreferences_Entry<bool> _displayMissCounts;
    private static MelonPreferences_Entry<bool> _displayAccuracy;
    private static MelonPreferences_Entry<bool> _displayScoreGap;
    private static MelonPreferences_Entry<bool> _displayHighestScore;
    private static MelonPreferences_Entry<string> _customSeparatist;

    // Upper right corner
    public static bool DisplaySongName => _displaySongName.Value;
    public static bool DisplaySongDifficulty => _displaySongDifficulty.Value;
    public static string CustomSongDifficultyFormat => string.IsNullOrEmpty(_customSongDifficultyFormat?.Value) ? "{diff} - Level {level}" : _customSongDifficultyFormat.Value;
    // Upper left corner
    public static bool DisplayHitCounts => _displayHitCounts.Value;
    public static string CustomHitCountsFormat => string.IsNullOrEmpty(_customHitCountsFormat?.Value) ? "{hit} of {total} notes" : _customHitCountsFormat.Value;
    public static bool DisplayMissCounts => _displayMissCounts.Value;
    public static bool DisplayAccuracy => _displayAccuracy.Value;
    public static bool DisplayScoreGap => _displayScoreGap.Value;
    public static bool DisplayHighestScore => _displayHighestScore.Value;
    public static string CustomSeparatist => string.IsNullOrEmpty(_customSeparatist?.Value) ? " / " : _customSeparatist.Value;

    public static string FinalSongDifficultyTextFormat { get; private set; }
    public static string FinalGameStatsTextFormat { get; private set; }
    public static string FinalScoreStatsTextFormat { get; private set; }
    public static string FinalHitStatsTextFormat { get; private set; }

    public static void Init()
    {
        _category = MelonPreferences.CreateCategory(ModBuildInfo.NAME, "Info Plus");
        _category.SetFilePath("UserData/Info+.cfg");

        _displaySongName = _category.CreateEntry("DisplaySongName", true, "Display the song name");
        _displaySongDifficulty = _category.CreateEntry("DisplaySongDifficulty", true, "Display the song difficulty");
        _customSongDifficultyFormat = _category.CreateEntry("CustomSongDifficultyFormat", CustomSongDifficultyFormat, "Custom song difficulty text format");

        _displayHitCounts = _category.CreateEntry("DisplayHitCounts", true, "Display hit counts");
        _customHitCountsFormat = _category.CreateEntry("CustomHitCountsFormat", CustomHitCountsFormat, "Custom hit counts text format");
        _displayMissCounts = _category.CreateEntry("DisplayMissCounts", true, "Display miss counts");
        _displayAccuracy = _category.CreateEntry("DisplayAccuracy", true, "Display current accuracy");
        _displayScoreGap = _category.CreateEntry("DisplayScoreGap", true, "Display score gap to highest score");
        _displayHighestScore = _category.CreateEntry("DisplayHighestScore", false, "Display highest score");
        _customSeparatist = _category.CreateEntry("CustomSeparatist", CustomSeparatist, "Custom separatist between counts");
    }

    public static void Load() => _category.LoadFromFile(false);

    public static void Save() => _category.SaveToFile(false);

    public static void ConstractTextFormats()
    {
        string separatist = $"<size={Constants.SEPARATIST_SIZE}><color={Constants.SEPARATIST_COLOR}>{CustomSeparatist}</color></size>";

        FinalSongDifficultyTextFormat = CustomSongDifficultyFormat;

        string format = string.Empty;
        if (DisplayAccuracy) format += "{acc}";
        if (DisplayMissCounts) format += (DisplayAccuracy ? separatist : string.Empty) + "{miss}";
        FinalGameStatsTextFormat = format.Trim();

        format = string.Empty;
        if (DisplayHighestScore) format += "{highest}";
        if (DisplayScoreGap) format += (DisplayHighestScore ? separatist : string.Empty) + "{gap}";
        FinalScoreStatsTextFormat = format.Trim();

        FinalHitStatsTextFormat = CustomHitCountsFormat;
    }
}
