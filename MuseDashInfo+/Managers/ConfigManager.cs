using MelonLoader;

using MDIP.Utils;
using Il2CppAssets.Scripts.PeroTools.Commons;

namespace MDIP.Managers;

public static class ConfigManager
{
    private static MelonPreferences_Category _category;
    // Upper right corner
    private static MelonPreferences_Entry<bool> _displayChartName;
    private static MelonPreferences_Entry<string> _chartNameColor;
    private static MelonPreferences_Entry<bool> _displayChartDifficulty;
    private static MelonPreferences_Entry<string> _customChartDifficultyFormat;
    // Upper left corner
    private static MelonPreferences_Entry<bool> _displayNoteCounts;
    private static MelonPreferences_Entry<string> _customNoteCountsFormat;
    private static MelonPreferences_Entry<bool> _displayMissCounts;
    private static MelonPreferences_Entry<string> _normalMissCountsColor;
    private static MelonPreferences_Entry<string> _collectableMissCountsColor;
    private static MelonPreferences_Entry<string> _greatCountsColor;
    private static MelonPreferences_Entry<bool> _displayAccuracy;
    private static MelonPreferences_Entry<bool> _displayHighestScore;
    private static MelonPreferences_Entry<bool> _displayScoreGap;
    private static MelonPreferences_Entry<string> _scoreGapAheadColor;
    private static MelonPreferences_Entry<string> _scoreGapBehindColor;
    // Others
    private static MelonPreferences_Entry<bool> _replaceResultsScreenMissCount;
    private static MelonPreferences_Entry<string> _customSeparator;
    private static MelonPreferences_Entry<string> _text1PositionOffset;
    private static MelonPreferences_Entry<string> _text2PositionOffset;
    private static MelonPreferences_Entry<string> _text3PositionOffset;

    // Upper right corner
    public static bool DisplayChartName => _displayChartName.Value;
    public static string ChartNameColor => string.IsNullOrEmpty(_chartNameColor?.Value) ? Constants.WHITE_COLOR : _chartNameColor.Value;
    public static bool DisplayChartDifficulty => _displayChartDifficulty.Value;
    public static string CustomChartDifficultyFormat => string.IsNullOrEmpty(_customChartDifficultyFormat?.Value) ? "{diff} - Level {level}" : _customChartDifficultyFormat.Value;
    // Upper left corner
    public static bool DisplayNoteCounts => _displayNoteCounts.Value;
    public static string CustomNoteCountsFormat => string.IsNullOrEmpty(_customNoteCountsFormat?.Value) ? "{hit} of {total} notes" : _customNoteCountsFormat.Value;
    public static bool DisplayMissCounts => _displayMissCounts.Value;
    public static string NormalMissCountsColor => string.IsNullOrEmpty(_normalMissCountsColor?.Value) ? Constants.WHITE_COLOR : _normalMissCountsColor.Value;
    public static string CollectableMissCountsColor => string.IsNullOrEmpty(_collectableMissCountsColor?.Value) ? Constants.WHITE_COLOR : _collectableMissCountsColor.Value;
    public static string GreatCountsColor => string.IsNullOrEmpty(_greatCountsColor?.Value) ? Constants.WHITE_COLOR : _greatCountsColor.Value;
    public static bool DisplayAccuracy => _displayAccuracy.Value;
    public static bool DisplayHighestScore => _displayHighestScore.Value;
    public static bool DisplayScoreGap => _displayScoreGap.Value;
    public static string ScoreGapAheadColor => string.IsNullOrEmpty(_scoreGapAheadColor?.Value) ? Constants.GAP_AHEAD_COLOR : _scoreGapAheadColor.Value;
    public static string ScoreGapBehindColor => string.IsNullOrEmpty(_scoreGapBehindColor?.Value) ? Constants.GAP_BEHIND_COLOR : _scoreGapBehindColor.Value;
    // Others
    public static bool ReplaceResultsScreenMissCount => _replaceResultsScreenMissCount.Value;
    public static string CustomSeparator => string.IsNullOrEmpty(_customSeparator?.Value) ? " / " : _customSeparator.Value;
    public static string Text1PositionOffset => string.IsNullOrEmpty(_text1PositionOffset?.Value) ? "0,0" : _text1PositionOffset.Value;
    public static string Text2PositionOffset => string.IsNullOrEmpty(_text2PositionOffset?.Value) ? "0,0" : _text2PositionOffset.Value;
    public static string Text3PositionOffset => string.IsNullOrEmpty(_text3PositionOffset?.Value) ? "0,0" : _text3PositionOffset.Value;

    public static string FinalChartDifficultyTextFormat { get; private set; }
    public static string FinalGameStatsTextFormat { get; private set; }
    public static string FinalScoreStatsTextFormat { get; private set; }
    public static string FinalNoteStatsTextFormat { get; private set; }

    public static void Init()
    {
        _category = MelonPreferences.CreateCategory(ModBuildInfo.NAME, "Info Plus");
        _category.SetFilePath("UserData/Info+.cfg");

        _displayChartName = _category.CreateEntry("DisplayChartName", !MDIPMod.IsSongDescLoaded, description: "Show song name\n显示歌曲名");
        _chartNameColor = _category.CreateEntry("ChartNameColor", ChartNameColor, description: "Song name color\n歌曲名颜色");
        _displayChartDifficulty = _category.CreateEntry("DisplayChartDifficulty", !MDIPMod.IsSongDescLoaded, description: "Show difficulty level\n显示谱面难度");
        _customChartDifficultyFormat = _category.CreateEntry("CustomChartDifficultyFormat", CustomChartDifficultyFormat, description: "Custom difficulty text format\n{diff} will be replaced with EASY/HARD/MASTER\n{level} will be replaced with the chart level\n自定义歌曲难度文本\n{diff} 将被替换为 EASY/HARD/MASTER\n{level} 将被替换为谱面等级");

        _displayNoteCounts = _category.CreateEntry("DisplayNoteCounts", true, description: "Show note counter\n显示物量计数器");
        _customNoteCountsFormat = _category.CreateEntry("CustomNoteCountsFormat", CustomNoteCountsFormat, description: "Custom note counter format\n{total} will be replaced with total notes\n{hit} will be replaced with current hit count\n自定义物量计数器文本\n{total} 将被替换为谱面总物量\n{hit} 将被替换为当前已击打数量");
        _displayMissCounts = _category.CreateEntry("DisplayMissCounts", true, description: "Show miss counter\n显示漏击计数器");
        _normalMissCountsColor = _category.CreateEntry("NormalMissCountsColor", NormalMissCountsColor, description: "Text color of missed count\n漏击计数的文本颜色");
        _collectableMissCountsColor = _category.CreateEntry("CollectableMissCountsColor", CollectableMissCountsColor, description: "Text color of collectable missed count\n音符/红心遗漏计数的文本颜色");
        _greatCountsColor = _category.CreateEntry("GreatCountsColor", GreatCountsColor, description: "Text color of great count\nGreat 计数的文本颜色");
        _displayAccuracy = _category.CreateEntry("DisplayAccuracy", true, description: "Show current accuracy\n显示当前准确率");
        _displayHighestScore = _category.CreateEntry("DisplayHighestScore", false, description: "Show personal best score\n显示当前谱面历史最高分数");
        _displayScoreGap = _category.CreateEntry("DisplayScoreGap", true, description: "Show score difference from high score\n显示当前与最高分的分数差距");
        _scoreGapAheadColor = _category.CreateEntry("ScoreGapAheadColor", ScoreGapAheadColor, description: "Score gap text color when higher than personal best\n当分数超过历史最高分时的分数差值文本颜色");
        _scoreGapBehindColor = _category.CreateEntry("ScoreGapBehindColor", ScoreGapBehindColor, description: "Score gap text color when lower than personal best\n当分数低于历史最高分时的分数差值文本颜色");

        _replaceResultsScreenMissCount = _category.CreateEntry("ReplaceResultsScreenMissCount", true, description: "Show mod-calculated misses instead of game's default count on results screen\n以模组计算的 Miss 数替代结算页面的 Miss 数");
        _customSeparator = _category.CreateEntry("CustomSeparator", CustomSeparator, description: "Custom separator between stats\n自定义各个数据之间的分隔符");
        _text1PositionOffset = _category.CreateEntry("Text1PositionOffset", Text1PositionOffset, description: "Position offset of custom text 1\n自定义文本1的位置偏移");
        _text2PositionOffset = _category.CreateEntry("Text2PositionOffset", Text2PositionOffset, description: "Position offset of custom text 2\n自定义文本2的位置偏移");
        _text3PositionOffset = _category.CreateEntry("Text3PositionOffset", Text3PositionOffset, description: "Position offset of custom text 3\n自定义文本3的位置偏移");
    }

    public static void Load() => _category.LoadFromFile(false);

    public static void Save() => _category.SaveToFile(false);

    public static void ConstractTextFormats()
    {
        string Separator = $"<size={Constants.SEPARATOR_SIZE}><color={Constants.SEPARATOR_COLOR}>{CustomSeparator}</color></size>";

        FinalChartDifficultyTextFormat = CustomChartDifficultyFormat;

        string format = string.Empty;
        if (DisplayAccuracy) format += "{acc}";
        if (DisplayMissCounts) format += (DisplayAccuracy ? Separator : string.Empty) + "{miss}";
        FinalGameStatsTextFormat = format.Trim();

        format = string.Empty;
        if (DisplayHighestScore) format += "{highest}";
        if (DisplayScoreGap) format += (DisplayHighestScore ? Separator : string.Empty) + "{gap}";
        FinalScoreStatsTextFormat = format.Trim();

        FinalNoteStatsTextFormat = CustomNoteCountsFormat;
    }
}
