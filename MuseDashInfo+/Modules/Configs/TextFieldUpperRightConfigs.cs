using MDIP.Interfaces;

namespace MDIP.Modules.Configs;

public class TextFieldUpperRightConfigs : ConfigBase, ITextConfig
{
    public bool Enabled { get; set; } = true;
    public string Text { get; set; } = "<b>{song}</b>\n<size=27>{diff} - Level {level}</size>";
    public float OffsetX { get; set; } = 0;
    public float OffsetY { get; set; } = 0;
    public string Font { get; set; } = "";
    public int FontSize { get; set; } = 38;
    public string FontColor { get; set; } = Utils.Constants.COLOR_WHITE;
    public bool FontOutlineEnabled { get; set; } = false;
    public string FontOutlineColor { get; set; } = Utils.Constants.COLOR_WHITE;
    public float FontOutlineWidth { get; set; } = 2;
}