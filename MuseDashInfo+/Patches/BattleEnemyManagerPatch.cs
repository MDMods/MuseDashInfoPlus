﻿using Il2CppAssets.Scripts.GameCore.HostComponent;
using MDIP.Modules.Enums;

namespace MDIP.Patches;

[HarmonyPatch(typeof(BattleEnemyManager), nameof(BattleEnemyManager.SetPlayResult))]
internal class BattleEnemyManagerSetPlayResultPatch
{
    private static void Postfix(int idx, byte result, bool isMulStart = false, bool isMulEnd = false, bool isLeft = false)
    {
        var note = GameStatsManager.GetMusicDataByIdx(idx);
        var type = (NoteType)note.noteData.type;
        var oid = note.objId;

        if (Configs.Advanced.OutputNoteRecordsToDesktop)
            NoteRecordManager.AddRecord(note, "SetPlayResult", $"result:{result}");

        switch (result)
        {
            case 4 when type == NoteType.Block:
                GameStatsManager.CountNote(oid, CountNoteAction.Block);
                break;
            case 1 when type == NoteType.Long:
                GameStatsManager.CountNote(oid, CountNoteAction.MissLong, -1, note.isLongPressStart);
                break;
        }

        if (!type.IsRegularNote() || note.isLongPressing)
            return;

        GameStatsManager.UpdateCurrentSpeed(note.isAir, note.noteData.speed);

        if (type == NoteType.Mul)
        {
            if (result is not 3 and not 4)
                return;
            GameStatsManager.UpdateCurrentStats();
            GameStatsManager.Mashing(note);
        }
        else
        {
            GameStatsManager.UpdateCurrentStats();
            GameStatsManager.CheckMashing();
            TextObjManager.UpdateAllText();
        }
    }
}

// Reference: https://github.com/flustix/AccDisplay/blob/main/AccDisplay/Patches/MissPatches.cs