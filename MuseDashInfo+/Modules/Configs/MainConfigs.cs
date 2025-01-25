using MDIP.Attributes;

namespace MDIP.Modules.Configs;

public class MainConfigs : ConfigBase
{
    [ConfigCommentZh("漏击计数的文本颜色")]
    [ConfigCommentEn("Text color of missed count")]
    public string NormalMissCountsColor { get; set; } = Utils.Constants.COLOR_WHITE;

    [ConfigCommentZh("音符/红心遗漏计数的文本颜色")]
    [ConfigCommentEn("Text color of collectable missed count")]
    public string CollectableMissCountsColor { get; set; } = Utils.Constants.COLOR_WHITE;

    [ConfigCommentZh("Great 计数的文本颜色")]
    [ConfigCommentEn("Text color of great count")]
    public string GreatCountsColor { get; set; } = Utils.Constants.COLOR_WHITE;

    [ConfigCommentZh("当分数超过历史最高分时的分数差值文本颜色")]
    [ConfigCommentEn("Score gap text color when higher than personal best")]
    public string ScoreGapAheadColor { get; set; } = Utils.Constants.COLOR_GAP_AHEAD;

    [ConfigCommentZh("当分数低于历史最高分时的分数差值文本颜色")]
    [ConfigCommentEn("Score gap text color when lower than personal best")]
    public string ScoreGapBehindColor { get; set; } = Utils.Constants.COLOR_GAP_BEHIND;

    [ConfigCommentZh("All-Perfect 指示文本")]
    [ConfigCommentEn("Text of All-Perfect indicator")]
    public string TextAllPerfect { get; set; } = Utils.Constants.TEXT_ALL_PERFECT;

    [ConfigCommentZh("理论值指示文本（0 Early & Late）")]
    [ConfigCommentEn("Text of True-Perfect indicator (0 Early & Late)")]
    public string TextTruePerfect { get; set; } = Utils.Constants.TEXT_TRUE_PERFECT;

    [ConfigCommentZh("SSS Rank 文本颜色")]
    [ConfigCommentEn("Text color of SSS rank")]
    public string RankSSSColor { get; set; } = Utils.Constants.COLOR_RANK_SSS;

    [ConfigCommentZh("SS Rank 文本颜色")]
    [ConfigCommentEn("Text color of SS rank")]
    public string RankSSColor { get; set; } = Utils.Constants.COLOR_RANK_SS;

    [ConfigCommentZh("S Rank 文本颜色")]
    [ConfigCommentEn("Text color of S rank")]
    public string RankSColor { get; set; } = Utils.Constants.COLOR_RANK_S;

    [ConfigCommentZh("A Rank 文本颜色")]
    [ConfigCommentEn("Text color of A rank")]
    public string RankAColor { get; set; } = Utils.Constants.COLOR_RANK_A;

    [ConfigCommentZh("B Rank 文本颜色")]
    [ConfigCommentEn("Text color of B rank")]
    public string RankBColor { get; set; } = Utils.Constants.COLOR_RANK_B;

    [ConfigCommentZh("C Rank 文本颜色")]
    [ConfigCommentEn("Text color of C rank")]
    public string RankCColor { get; set; } = Utils.Constants.COLOR_RANK_C;

    [ConfigCommentZh("D Rank 文本颜色")]
    [ConfigCommentEn("Text color of D rank")]
    public string RankDColor { get; set; } = Utils.Constants.COLOR_RANK_D;

    [ConfigCommentZh("EASY 难度文本（支持富文本格式）")]
    [ConfigCommentEn("Text of EASY difficulty (supports rich text format)")]
    public string TextDiff1 { get; set; } = Utils.Constants.TEXT_DIFF_1;

    [ConfigCommentZh("HARD 难度文本（支持富文本格式）")]
    [ConfigCommentEn("Text of HARD difficulty (supports rich text format)")]
    public string TextDiff2 { get; set; } = Utils.Constants.TEXT_DIFF_2;

    [ConfigCommentZh("MASTER 难度文本（支持富文本格式）")]
    [ConfigCommentEn("Text of MASTER difficulty (supports rich text format)")]
    public string TextDiff3 { get; set; } = Utils.Constants.TEXT_DIFF_3;

    [ConfigCommentZh("HIDDEN 难度文本（支持富文本格式）")]
    [ConfigCommentEn("Text of HIDDEN difficulty (supports rich text format)")]
    public string TextDiff4 { get; set; } = Utils.Constants.TEXT_DIFF_4;

    [ConfigCommentZh("SPECIAL 难度文本（支持富文本格式）")]
    [ConfigCommentEn("Text of SPECIAL difficulty (supports rich text format)")]
    public string TextDiff5 { get; set; } = Utils.Constants.TEXT_DIFF_5;

    [ConfigCommentZh("以模组计算的 Miss 数替代结算页面的 Miss 数")]
    [ConfigCommentEn("Show mod-calculated misses instead of game's default count on results screen")]
    public bool ReplaceResultsScreenMissCount { get; set; } = true;
}