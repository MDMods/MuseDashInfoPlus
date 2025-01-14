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
    private static MelonPreferences_Entry<string> _customSeparator;

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
    public static string CustomSeparator => string.IsNullOrEmpty(_customSeparator?.Value) ? " / " : _customSeparator.Value;

    public static string FinalSongDifficultyTextFormat { get; private set; }
    public static string FinalGameStatsTextFormat { get; private set; }
    public static string FinalScoreStatsTextFormat { get; private set; }
    public static string FinalHitStatsTextFormat { get; private set; }

    public static void Init()
    {
        _category = MelonPreferences.CreateCategory(ModBuildInfo.NAME, "Info Plus");
        _category.SetFilePath("UserData/Info+.cfg");

        _displaySongName = _category.CreateEntry("DisplaySongName", true, description: "Show song name\n显示歌曲名");
        _displaySongDifficulty = _category.CreateEntry("DisplaySongDifficulty", true, description: "Show difficulty level\n显示谱面难度");
        _customSongDifficultyFormat = _category.CreateEntry("CustomSongDifficultyFormat", CustomSongDifficultyFormat, description: "Custom difficulty text format\n{diff} will be replaced with EASY/HARD/MASTER\n{level} will be replaced with the chart level\n自定义歌曲难度文本\n{diff} 将被替换为 EASY/HARD/MASTER\n{level} 将被替换为谱面等级");

        _displayHitCounts = _category.CreateEntry("DisplayHitCounts", true, description: "Show note counter\n显示物量计数器");
        _customHitCountsFormat = _category.CreateEntry("CustomHitCountsFormat", CustomHitCountsFormat, description: "Custom note counter format\n{total} will be replaced with total notes\n{hit} will be replaced with current hit count\n自定义物量计数器文本\n{total} 将被替换为谱面总物量\n{hit} 将被替换为当前已击打数量");
        _displayMissCounts = _category.CreateEntry("DisplayMissCounts", true, description: "Show miss counter\n显示漏击计数器");
        _displayAccuracy = _category.CreateEntry("DisplayAccuracy", true, description: "Show current accuracy\n显示当前准确率");
        _displayScoreGap = _category.CreateEntry("DisplayScoreGap", true, description: "Show score difference from high score\n显示当前与最高分的分数差距");
        _displayHighestScore = _category.CreateEntry("DisplayHighestScore", false, description: "Show historical high score\n显示当前谱面历史最高分数");
        _customSeparator = _category.CreateEntry("CustomSeparator", CustomSeparator, description: "Custom separator between stats\n自定义各个数据之间的分隔符");
    }

    public static void Load() => _category.LoadFromFile(false);

    public static void Save() => _category.SaveToFile(false);

    public static void ConstractTextFormats()
    {
        string Separator = $"<size={Constants.Separator_SIZE}><color={Constants.Separator_COLOR}>{CustomSeparator}</color></size>";

        FinalSongDifficultyTextFormat = CustomSongDifficultyFormat;

        string format = string.Empty;
        if (DisplayAccuracy) format += "{acc}";
        if (DisplayMissCounts) format += (DisplayAccuracy ? Separator : string.Empty) + "{miss}";
        FinalGameStatsTextFormat = format.Trim();

        format = string.Empty;
        if (DisplayHighestScore) format += "{highest}";
        if (DisplayScoreGap) format += (DisplayHighestScore ? Separator : string.Empty) + "{gap}";
        FinalScoreStatsTextFormat = format.Trim();

        FinalHitStatsTextFormat = CustomHitCountsFormat;
    }
}
