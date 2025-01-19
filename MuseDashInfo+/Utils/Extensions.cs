using Il2CppGameLogic;
using System;

using MDIP.Modules;

namespace MDIP.Utils;

public static class Extensions
{
    public static string Color(this string text, string color) => $"<color={color}>{text}</color>";

    public static bool IsRegularNote(this NoteType noteType) => Utils.IsRegularNote((uint)noteType);

    public static int Count(this Il2CppSystem.Collections.Generic.List<MusicData> noteList, Func<MusicData, bool> predicate)
    {
        int count = 0;
        foreach (var note in noteList)
        {
            if (predicate(note))
                count++;
        }
        return count;
    }
}
