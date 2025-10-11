using MDIP.Core.Domain.Attributes;

namespace MDIP.Core.Domain.Configs;

public class AdvancedConfigs : ConfigBase
{
    [ConfigCommentZh("数据刷新间隔（毫秒）")]
    [ConfigCommentEn("Data refresh interval (milliseconds)")]
    public int DataRefreshIntervalMs { get; set; } = 123;

    [ConfigCommentZh("文本刷新间隔（毫秒，<=0 则与数据间隔一致；不会快于数据间隔）")]
    [ConfigCommentEn("Text refresh interval (ms, <=0 means same as data; will not be faster than data)")]
    public int TextRefreshIntervalMs { get; set; } = 0;

    [ConfigCommentZh("在控制台显示 Note 调试数据")]
    [ConfigCommentEn("Display note debugging data in the console")]
    public bool DisplayNoteDebuggingData { get; set; } = false;

    [ConfigCommentZh("游戏结算时输出 Note 记录到桌面")]
    [ConfigCommentEn("Output note records file to desktop when the game is settled")]
    public bool OutputNoteRecordsToDesktop { get; set; } = false;
}