using MelonLoader;
using System;

using MDIP.Modules;

namespace MDIP.Utils;

public static class Utils
{
    public static bool IsRegularNote(this NoteType noteType) => IsRegularNote((uint)noteType);
    public static bool IsRegularNote(uint noteType) => noteType >= 1 && noteType <= 8;

    public static Func<Il2CppGameLogic.MusicData, bool> IsSingleNoteFunc = new(note => IsRegularNote(note.noteData.type) && !note.isLongPressing);

    public static int Count(this Il2CppSystem.Collections.Generic.List<Il2CppGameLogic.MusicData> noteList, Func<Il2CppGameLogic.MusicData, bool> predicate)
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