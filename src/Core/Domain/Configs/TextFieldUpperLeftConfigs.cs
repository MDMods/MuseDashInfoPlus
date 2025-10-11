namespace MDIP.Core.Domain.Configs;

public class TextFieldUpperLeftConfigs : ConfigBase, ITextConfig
{
    public bool Enabled { get; set; } = true;
    public string Text { get; set; } = "<b>{overview}</b>";
    public float OffsetX { get; set; } = 20;
    public float OffsetY { get; set; } = 30;
    public string Font { get; set; } = "Snaps Taste";
    public int FontSize { get; set; } = 60;
    public string FontColor { get; set; } = Constants.Constants.COLOR_WHITE;
    public bool FontOutlineEnabled { get; set; } = false;
    public string FontOutlineColor { get; set; } = Constants.Constants.COLOR_WHITE;
    public float FontOutlineWidth { get; set; } = 3;
}