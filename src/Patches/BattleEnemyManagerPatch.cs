using Il2CppAssets.Scripts.GameCore.HostComponent;

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
            NoteRecordManager.AddRecord(note, "SetPlayResult", $"setResult:{result}");

        switch (result)
        {
            case 4 when type == NoteType.Block:
                GameStatsManager.CountNote(oid, CountNoteAction.Block);
                break;
            case 1 when type == NoteType.Long:
                GameStatsManager.CountNote(oid, CountNoteAction.MissLong, -1, note.isLongPressStart);
                break;
        }

        if (type == NoteType.Mul)
            GameStatsManager.CountMul(oid, result, (float)note.configData.length);

        if (!type.IsRegularNote() || note.isLongPressing)
            return;

        GameStatsManager.UpdateCurrentSpeed(note.isAir, note.noteData.speed);
        GameStatsManager.UpdateCurrentStats();
        TextObjManager.UpdateAllText();
    }
}

// Reference: https://github.com/flustix/AccDisplay/blob/main/AccDisplay/Patches/MissPatches.cs