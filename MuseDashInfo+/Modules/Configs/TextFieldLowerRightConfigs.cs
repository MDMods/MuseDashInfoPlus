using MDIP.Interfaces;

namespace MDIP.Modules.Configs;

public class TextFieldLowerRightConfigs : ConfigBase, ITextConfig
{
	public bool Enabled { get; set; } = false;
	public string Text { get; set; } = "Sky Speed: {skySpeed}x\\nGround Speed: {groundSpeed}x";
	public float OffsetX { get; set; } = 0;
	public float OffsetY { get; set; } = 0;
	public string Font { get; set; } = "Lato";
	public int FontSize { get; set; } = 30;
	public string FontColor { get; set; } = Constants.COLOR_WHITE;
	public bool FontOutlineEnabled { get; set; } = false;
	public string FontOutlineColor { get; set; } = Constants.COLOR_WHITE;
	public float FontOutlineWidth { get; set; } = 2;
}