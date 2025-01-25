using MDIP.Interfaces;

namespace MDIP.Modules.Configs;

public class TextFieldLowerLeftConfigs : ConfigBase, ITextConfig
{
    public bool Enabled { get; set; } = true;
    public string Text { get; set; } = "{hit} of {total} notes";
    public float OffsetX { get; set; } = 0;
    public float OffsetY { get; set; } = 0;
    public string Font { get; set; } = "Lato";
    public int FontSize { get; set; } = 30;
    public string FontColor { get; set; } = Utils.Constants.COLOR_WHITE;
    public bool FontOutlineEnabled { get; set; } = false;
    public string FontOutlineColor { get; set; } = Utils.Constants.COLOR_WHITE;
    public float FontOutlineWidth { get; set; } = 2;
}