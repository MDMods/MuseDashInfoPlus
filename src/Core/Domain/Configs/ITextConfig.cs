using MDIP.Core.Domain.Attributes;

namespace MDIP.Core.Domain.Configs;

public interface ITextConfig
{
    [ConfigCommentZh("启用该文本区域")]
    [ConfigCommentEn("Enable this text field")]
    bool Enabled { get; set; }

    [ConfigCommentZh("该文本区域的文本内容（支持富文本格式）\n使用 {data} 将被替换为指定数据")]
    [ConfigCommentEn("Text content of this text field (supports rich text format)\n{data} will be replaced with specified data")]
    string Text { get; set; }

    [ConfigCommentZh("该文本区域的横坐标位置偏移")]
    [ConfigCommentEn("Horizontal offset of this text field")]
    float OffsetX { get; set; }

    [ConfigCommentZh("该文本区域的纵坐标位置偏移")]
    [ConfigCommentEn("Vertical offset of this text field")]
    float OffsetY { get; set; }

    [ConfigCommentZh("该文本区域的字体")]
    [ConfigCommentEn("Font of this text field")]
    string Font { get; set; }

    [ConfigCommentZh("该文本区域的默认字体大小")]
    [ConfigCommentEn("Default font size of this text field")]
    int FontSize { get; set; }

    [ConfigCommentZh("该文本区域的默认字体颜色")]
    [ConfigCommentEn("Default font color of this text field")]
    string FontColor { get; set; }

    [ConfigCommentZh("启用该文本区域的字体描边")]
    [ConfigCommentEn("Enable font outline of this text field")]
    bool FontOutlineEnabled { get; set; }

    [ConfigCommentZh("该文本区域的字体描边颜色")]
    [ConfigCommentEn("Font outline color of this text field")]
    string FontOutlineColor { get; set; }

    [ConfigCommentZh("该文本区域的字体描边宽度")]
    [ConfigCommentEn("Font outline width of this text field")]
    float FontOutlineWidth { get; set; }
}