using MDIP.Interfaces;

namespace MDIP.Modules.Configs;

public class TextFieldUpperLeftConfigs : ConfigBase, ITextConfig
{
    public bool Enabled { get; set; } = false;
    public string Text { get; set; } = "{acc}";
    public float OffsetX { get; set; } = 0;
    public float OffsetY { get; set; } = 0;
    public string Font { get; set; } = "Snaps Taste";
    public int FontSize { get; set; } = 50;
    public string FontColor { get; set; } = Utils.Constants.COLOR_WHITE;
    public bool FontOutlineEnabled { get; set; } = false;
    public string FontOutlineColor { get; set; } = Utils.Constants.COLOR_WHITE;
    public float FontOutlineWidth { get; set; } = 3;
}