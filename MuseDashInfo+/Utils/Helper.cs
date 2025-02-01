using System;
using System.Numerics;
using Il2CppGameLogic;
using MelonLoader;

namespace MDIP.Utils;

public static class Helper
{
	public static Func<MusicData, bool> IsSingleNoteFunc = note => IsRegularNote(note.noteData.type) && !note.isLongPressing;

	public static bool AmDebugger
	{
#if DEBUG
		get => true;
#else
		get => false;
#endif
	}

	public static bool OutputAccuracyCalculationDatas => AmDebugger || Configs.Advanced.OutputAccuracyCalculationData;

	public static bool OutputNoteRecordsToDesktop => AmDebugger || Configs.Advanced.OutputNoteRecordsToDesktop;
	public static bool IsRegularNote(uint noteType) => noteType >= 1 && noteType <= 8;

	public static Vector2 StringToVector2(string text)
	{
		try
		{
			if (string.IsNullOrEmpty(text))
				return Vector2.Zero;

			text = text.Replace(" ", "").Replace("，", ",");

			var parts = text.Split(',');

			if (parts.Length != 2)
				return Vector2.Zero;

			var x = float.Parse(parts[0]);
			var y = float.Parse(parts[1]);

			return new(x, y);
		}
		catch
		{
			Melon<MDIPMod>.Logger.Error($"The custom text position offset ({text}) you set is in the wrong format");
			Melon<MDIPMod>.Logger.Error($"您设置的自定义文本位置（{text}）格式错误");
			return Vector2.Zero;
		}
	}
}