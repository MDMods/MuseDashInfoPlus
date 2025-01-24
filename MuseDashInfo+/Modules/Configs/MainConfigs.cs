using MDIP.Utils.Attributes;

namespace MDIP.Modules.Configs;

public class MainConfigs : ConfigBase
{
    [ConfigComment("显示歌曲名", "Show song name")]
    public bool DisplayChartName { get; set; } = !MDIPMod.IsSongDescLoaded;

    [ConfigComment("歌曲名颜色", "Song name color")]
    public string ChartNameColor { get; set; } = Utils.Constants.WHITE_COLOR;

    [ConfigComment("显示谱面难度", "Show difficulty level")]
    public bool DisplayChartDifficulty { get; set; } = !MDIPMod.IsSongDescLoaded;

    [ConfigComment("自定义歌曲难度文本\n{diff} 将被替换为 EASY/HARD/MASTER\n{level} 将被替换为谱面等级", "Custom difficulty text format\n{diff} will be replaced with EASY/HARD/MASTER\n{level} will be replaced with the chart level")]
    public string CustomChartDifficultyFormat { get; set; } = "{diff} - Level {level}";

    [ConfigComment("显示物量计数器", "Show note counter")]
    public bool DisplayNoteCounts { get; set; } = true;

    [ConfigComment("自定义物量计数器文本\n{total} 将被替换为谱面总物量\n{hit} 将被替换为当前已击打数量", "Custom note counter format\n{total} will be replaced with total notes\n{hit} will be replaced with current hit count")]
    public string CustomNoteCountsFormat { get; set; } = "{hit} of {total} notes";

    [ConfigComment("显示漏击计数器", "Show miss counter")]
    public bool DisplayMissCounts { get; set; } = true;

    [ConfigComment("漏击计数的文本颜色", "Text color of missed count")]
    public string NormalMissCountsColor { get; set; } = Utils.Constants.WHITE_COLOR;

    [ConfigComment("音符/红心遗漏计数的文本颜色", "Text color of collectable missed count")]
    public string CollectableMissCountsColor { get; set; } = Utils.Constants.WHITE_COLOR;

    [ConfigComment("Great 计数的文本颜色", "Text color of great count")]
    public string GreatCountsColor { get; set; } = Utils.Constants.WHITE_COLOR;

    [ConfigComment("显示当前准确率", "Show current accuracy")]
    public bool DisplayAccuracy { get; set; } = true;

    [ConfigComment("显示当前谱面历史最高分数", "Show personal best score")]
    public bool DisplayHighestScore { get; set; } = false;

    [ConfigComment("显示当前与最高分的分数差距", "Show score difference from high score")]
    public bool DisplayScoreGap { get; set; } = true;

    [ConfigComment("当分数超过历史最高分时的分数差值文本颜色", "Score gap text color when higher than personal best")]
    public string ScoreGapAheadColor { get; set; } = Utils.Constants.GAP_AHEAD_COLOR;

    [ConfigComment("当分数低于历史最高分时的分数差值文本颜色", "Score gap text color when lower than personal best")]
    public string ScoreGapBehindColor { get; set; } = Utils.Constants.GAP_BEHIND_COLOR;

    [ConfigComment("以模组计算的 Miss 数替代结算页面的 Miss 数", "Show mod-calculated misses instead of game's default count on results screen")]
    public bool ReplaceResultsScreenMissCount { get; set; } = true;

    [ConfigComment("自定义各个数据之间的分隔符", "Custom separator between stats")]
    public string CustomSeparator { get; set; } = " / ";

    [ConfigComment("自定义文本1的位置偏移", "Position offset of text 1")]
    public string Text1PositionOffset { get; set; } = "0,0";

    [ConfigComment("自定义文本2的位置偏移", "Position offset of text 2")]
    public string Text2PositionOffset { get; set; } = "0,0";

    [ConfigComment("自定义文本3的位置偏移", "Position offset of text 3")]
    public string Text3PositionOffset { get; set; } = "0,0";
}
