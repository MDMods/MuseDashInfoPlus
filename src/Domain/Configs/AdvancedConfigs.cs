using MDIP.Domain.Attributes;

namespace MDIP.Domain.Configs;

public class AdvancedConfigs : ConfigBase
{
    [ConfigCommentZh("数据刷新间隔限制（毫秒）")]
    [ConfigCommentEn("Data refresh interval limit (milliseconds)")]
    public int DataRefreshIntervalLimit { get; set; } = 123;

    [ConfigCommentZh("在控制台显示 Note 调试数据")]
    [ConfigCommentEn("Display note debugging data in the console")]
    public bool DisplayNoteDebuggingData { get; set; } = false;

    [ConfigCommentZh("游戏结算时输出 Note 记录到桌面")]
    [ConfigCommentEn("Output note records file to desktop when the game is settled")]
    public bool OutputNoteRecordsToDesktop { get; set; } = false;
}