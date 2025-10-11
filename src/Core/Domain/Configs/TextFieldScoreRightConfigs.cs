namespace MDIP.Core.Domain.Configs;

public class TextFieldScoreRightConfigs : ConfigBase, ITextConfig
{
    public bool Enabled { get; set; } = true;
    public string Text { get; set; } = "{scoreGap}";
    public float OffsetX { get; set; } = 0;
    public float OffsetY { get; set; } = 0;
    public string Font { get; set; } = "Snaps Taste";
    public int FontSize { get; set; } = 38;
    public string FontColor { get; set; } = Constants.Constants.COLOR_WHITE;
    public bool FontOutlineEnabled { get; set; } = false;
    public string FontOutlineColor { get; set; } = Constants.Constants.COLOR_WHITE;
    public float FontOutlineWidth { get; set; } = 2;
}