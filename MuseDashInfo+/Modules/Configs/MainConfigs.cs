using MDIP.Attributes;

namespace MDIP.Modules.Configs;

public class MainConfigs : ConfigBase
{
    [ConfigCommentZh("显示歌曲名")]
    [ConfigCommentEn("Show song name")]
    public bool DisplayChartName { get; set; } = !MDIPMod.IsSongDescLoaded;

    [ConfigCommentZh("歌曲名颜色")]
    [ConfigCommentEn("Song name color")]
    public string ChartNameColor { get; set; } = Utils.Constants.WHITE_COLOR;

    [ConfigCommentZh("显示谱面难度")]
    [ConfigCommentEn("Show difficulty level")]
    public bool DisplayChartDifficulty { get; set; } = !MDIPMod.IsSongDescLoaded;

    [ConfigCommentZh("自定义歌曲难度文本\n{diff} 将被替换为 EASY/HARD/MASTER\n{level} 将被替换为谱面等级")]
    [ConfigCommentEn("Custom difficulty text format\n{diff} will be replaced with EASY/HARD/MASTER\n{level} will be replaced with the chart level")]
    public string CustomChartDifficultyFormat { get; set; } = "{diff} - Level {level}";

    [ConfigCommentZh("显示物量计数器")][ConfigCommentEn("Show note counter")]
    public bool DisplayNoteCounts { get; set; } = true;

    [ConfigCommentZh("自定义物量计数器文本\n{total} 将被替换为谱面总物量\n{hit} 将被替换为当前已击打数量")]
    [ConfigCommentEn("Custom note counter format\n{total} will be replaced with total notes\n{hit} will be replaced with current hit count")]
    public string CustomNoteCountsFormat { get; set; } = "{hit} of {total} notes";

    [ConfigCommentZh("显示漏击计数器")]
    [ConfigCommentEn("Show miss counter")]
    public bool DisplayMissCounts { get; set; } = true;

    [ConfigCommentZh("漏击计数的文本颜色")]
    [ConfigCommentEn("Text color of missed count")]
    public string NormalMissCountsColor { get; set; } = Utils.Constants.WHITE_COLOR;

    [ConfigCommentZh("音符/红心遗漏计数的文本颜色")]
    [ConfigCommentEn("Text color of collectable missed count")]
    public string CollectableMissCountsColor { get; set; } = Utils.Constants.WHITE_COLOR;

    [ConfigCommentZh("Great 计数的文本颜色")]
    [ConfigCommentEn("Text color of great count")]
    public string GreatCountsColor { get; set; } = Utils.Constants.WHITE_COLOR;

    [ConfigCommentZh("显示当前准确率")]
    [ConfigCommentEn("Show current accuracy")]
    public bool DisplayAccuracy { get; set; } = true;

    [ConfigCommentZh("显示当前谱面历史最高分数")]
    [ConfigCommentEn("Show personal best score")]
    public bool DisplayHighestScore { get; set; } = false;

    [ConfigCommentZh("显示当前与最高分的分数差距")]
    [ConfigCommentEn("Show score difference from high score")]
    public bool DisplayScoreGap { get; set; } = true;

    [ConfigCommentZh("当分数超过历史最高分时的分数差值文本颜色")]
    [ConfigCommentEn("Score gap text color when higher than personal best")]
    public string ScoreGapAheadColor { get; set; } = Utils.Constants.GAP_AHEAD_COLOR;

    [ConfigCommentZh("当分数低于历史最高分时的分数差值文本颜色")]
    [ConfigCommentEn("Score gap text color when lower than personal best")]
    public string ScoreGapBehindColor { get; set; } = Utils.Constants.GAP_BEHIND_COLOR;

    [ConfigCommentZh("以模组计算的 Miss 数替代结算页面的 Miss 数")]
    [ConfigCommentEn("Show mod-calculated misses instead of game's default count on results screen")]
    public bool ReplaceResultsScreenMissCount { get; set; } = true;

    [ConfigCommentZh("自定义各个数据之间的分隔符")]
    [ConfigCommentEn("Custom separator between stats")]
    public string CustomSeparator { get; set; } = " / ";

    [ConfigCommentZh("自定义文本1的位置偏移")]
    [ConfigCommentEn("Position offset of text 1")]
    public string Text1PositionOffset { get; set; } = "0,0";

    [ConfigCommentZh("自定义文本2的位置偏移")]
    [ConfigCommentEn("Position offset of text 2")]
    public string Text2PositionOffset { get; set; } = "0,0";

    [ConfigCommentZh("自定义文本3的位置偏移")]
    [ConfigCommentEn("Position offset of text 3")]
    public string Text3PositionOffset { get; set; } = "0,0";
}
