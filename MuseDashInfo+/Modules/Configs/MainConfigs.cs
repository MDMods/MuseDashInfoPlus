using MDIP.Attributes;

namespace MDIP.Modules.Configs;

public class MainConfigs : ConfigBase
{
	[ConfigCommentZh("准确率显示模式\n1：从100%倒减\n2：实时计算")]
	[ConfigCommentEn("Accuracy display mode\n1: Subtract from 100%\n2: Calculate in real-time")]
	public int AccuracyDisplayMode { get; set; } = 1;

	[ConfigCommentZh("漏击计数的文本颜色")]
	[ConfigCommentEn("Accuracy text color of missed count")]
	public string NormalMissCountsColor { get; set; } = Constants.COLOR_WHITE;

	[ConfigCommentZh("音符/红心遗漏计数的文本颜色")]
	[ConfigCommentEn("Accuracy text color of collectable missed count")]
	public string CollectableMissCountsColor { get; set; } = Constants.COLOR_WHITE;

	[ConfigCommentZh("Great 计数的文本颜色")]
	[ConfigCommentEn("Accuracy text color of great count")]
	public string GreatCountsColor { get; set; } = Constants.COLOR_WHITE;

	[ConfigCommentZh("当分数超过历史最高分时的分数差值文本颜色")]
	[ConfigCommentEn("Score gap text color when higher than personal best")]
	public string ScoreGapAheadColor { get; set; } = Constants.COLOR_GAP_AHEAD;

	[ConfigCommentZh("当分数低于历史最高分时的分数差值文本颜色")]
	[ConfigCommentEn("Score gap text color when lower than personal best")]
	public string ScoreGapBehindColor { get; set; } = Constants.COLOR_GAP_BEHIND;

	[ConfigCommentZh("All-Perfect 指示文本（支持富文本格式）")]
	[ConfigCommentEn("Text of All-Perfect indicator (supports rich text format)")]
	public string TextAllPerfect { get; set; } = Constants.TEXT_ALL_PERFECT;

	[ConfigCommentZh("理论值（0 Early & Late）指示文本（支持富文本格式）")]
	[ConfigCommentEn("Text of True-Perfect (0 Early & Late) indicator (supports rich text format)")]
	public string TextTruePerfect { get; set; } = Constants.TEXT_TRUE_PERFECT;

	[ConfigCommentZh("显示 Early & Late 计数器")]
	[ConfigCommentEn("Show Early & Late counts")]
	public bool ShowEarlyLateCounts { get; set; } = true;

	[ConfigCommentZh("Early & Late 计数器的显示模式\n1：仅在 AP 时显示\n2：总是显示")]
	[ConfigCommentEn("Display mode of Early & Late counts\n1: Show only when AP\n2: Always show")]
	public int EarlyLateCountsDisplayMode { get; set; } = 1;

	[ConfigCommentZh("Early 计数器的文本颜色")]
	[ConfigCommentEn("Text color of Early count")]
	public string EarlyCountsColor { get; set; } = Constants.COLOR_EARLY_COUNT;

	[ConfigCommentZh("Late 计数器的文本颜色")]
	[ConfigCommentEn("Text color of Late count")]
	public string LateCountsColor { get; set; } = Constants.COLOR_LATE_COUNT;

	[ConfigCommentZh("TP 准确度文本颜色")]
	[ConfigCommentEn("Accuracy text color of TP rank")]
	public string RankTPColor { get; set; } = Constants.COLOR_RANK_TP;

	[ConfigCommentZh("AP 准确度文本颜色")]
	[ConfigCommentEn("Accuracy text color of AP rank")]
	public string RankAPColor { get; set; } = Constants.COLOR_RANK_AP;

	[ConfigCommentZh("SS 准确度文本颜色")]
	[ConfigCommentEn("Accuracy text color of SS rank")]
	public string RankSSColor { get; set; } = Constants.COLOR_RANK_SS;

	[ConfigCommentZh("S 准确度文本颜色")]
	[ConfigCommentEn("Accuracy text color of S rank")]
	public string RankSColor { get; set; } = Constants.COLOR_RANK_S;

	[ConfigCommentZh("A 准确度文本颜色")]
	[ConfigCommentEn("Accuracy text color of A rank")]
	public string RankAColor { get; set; } = Constants.COLOR_RANK_A;

	[ConfigCommentZh("B 准确度文本颜色")]
	[ConfigCommentEn("Accuracy text color of B rank")]
	public string RankBColor { get; set; } = Constants.COLOR_RANK_B;

	[ConfigCommentZh("C 准确度文本颜色")]
	[ConfigCommentEn("Accuracy text color of C rank")]
	public string RankCColor { get; set; } = Constants.COLOR_RANK_C;

	[ConfigCommentZh("D 准确度文本颜色")]
	[ConfigCommentEn("Accuracy text color of D rank")]
	public string RankDColor { get; set; } = Constants.COLOR_RANK_D;

	[ConfigCommentZh("EASY 难度文本（支持富文本格式）")]
	[ConfigCommentEn("Text of EASY difficulty (supports rich text format)")]
	public string TextDiff1 { get; set; } = Constants.TEXT_DIFF_1;

	[ConfigCommentZh("HARD 难度文本（支持富文本格式）")]
	[ConfigCommentEn("Text of HARD difficulty (supports rich text format)")]
	public string TextDiff2 { get; set; } = Constants.TEXT_DIFF_2;

	[ConfigCommentZh("MASTER 难度文本（支持富文本格式）")]
	[ConfigCommentEn("Text of MASTER difficulty (supports rich text format)")]
	public string TextDiff3 { get; set; } = Constants.TEXT_DIFF_3;

	[ConfigCommentZh("HIDDEN 难度文本（支持富文本格式）")]
	[ConfigCommentEn("Text of HIDDEN difficulty (supports rich text format)")]
	public string TextDiff4 { get; set; } = Constants.TEXT_DIFF_4;

	[ConfigCommentZh("SPECIAL 难度文本（支持富文本格式）")]
	[ConfigCommentEn("Text of SPECIAL difficulty (supports rich text format)")]
	public string TextDiff5 { get; set; } = Constants.TEXT_DIFF_5;

	[ConfigCommentZh("以模组计算的 Miss 数替代结算页面的 Miss 数")]
	[ConfigCommentEn("Show mod-calculated misses instead of game's default count on results screen")]
	public bool ReplaceResultsScreenMissCount { get; set; } = false;
}