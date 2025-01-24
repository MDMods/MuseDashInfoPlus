using MDIP.Attributes;

namespace MDIP.Modules.Configs;

public class AdvancedConfigs : ConfigBase
{
    [ConfigComment("显示准确率计算数据", "Output accuracy calculation datas")]
    public bool OutputAccuracyCalculationData { get; set; } = false;

    [ConfigComment("输出Note记录到桌面", "Output Note records to desktop")]
    public bool OutputNoteRecordsToDesktop { get; set; } = false;
}
