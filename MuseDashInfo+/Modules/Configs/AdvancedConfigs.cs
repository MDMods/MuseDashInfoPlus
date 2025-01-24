using MDIP.Attributes;

namespace MDIP.Modules.Configs;

public class AdvancedConfigs : ConfigBase
{
    [ConfigCommentZh("显示准确率计算数据")]
    [ConfigCommentEn("Output accuracy calculation datas")]
    public bool OutputAccuracyCalculationData { get; set; } = false;

    [ConfigCommentZh("输出Note记录到桌面")]
    [ConfigCommentEn("Output Note records to desktop")]
    public bool OutputNoteRecordsToDesktop { get; set; } = false;
}
