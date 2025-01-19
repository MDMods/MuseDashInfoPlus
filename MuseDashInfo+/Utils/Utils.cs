using Il2CppGameLogic;
using System.Numerics;
using System;
using MelonLoader;

namespace MDIP.Utils;

public static class Utils
{
    public static bool IsRegularNote(uint noteType) => noteType >= 1 && noteType <= 8;

    public static Func<MusicData, bool> IsSingleNoteFunc = new(note => IsRegularNote(note.noteData.type) && !note.isLongPressing);

    public static Vector2 StringToVector2(string text)
    {
        try
        {
            if (string.IsNullOrEmpty(text))
                return Vector2.Zero;

            text = text.Replace(" ", "").Replace("，", ",");

            string[] parts = text.Split(',');

            if (parts.Length != 2)
                return Vector2.Zero;

            float x = float.Parse(parts[0]);
            float y = float.Parse(parts[1]);

            return new Vector2(x, y);
        }
        catch
        {
            Melon<MDIPMod>.Logger.Error($"The custom text position offset ({text}) you set is in the wrong format");
            Melon<MDIPMod>.Logger.Error($"您设置的自定义文本位置（{text}）格式错误");
            return Vector2.Zero;
        }
    }
}