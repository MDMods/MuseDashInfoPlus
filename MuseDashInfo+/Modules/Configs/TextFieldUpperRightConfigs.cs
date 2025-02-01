using MDIP.Interfaces;
using MDIP.Utils;

namespace MDIP.Modules.Configs;

public class TextFieldUpperRightConfigs : ConfigBase, ITextConfig
{
	public bool Enabled { get; set; } = !MDIPMod.IsSongDescLoaded;
	public string Text { get; set; } = "{song}\\n<size=27>{diff} - Level {level}</size>";
	public float OffsetX { get; set; } = 0;
	public float OffsetY { get; set; } = 0;
	public string Font { get; set; } = "Normal";
	public int FontSize { get; set; } = 38;
	public string FontColor { get; set; } = Constants.COLOR_WHITE;
	public bool FontOutlineEnabled { get; set; } = false;
	public string FontOutlineColor { get; set; } = Constants.COLOR_WHITE;
	public float FontOutlineWidth { get; set; } = 2;
}