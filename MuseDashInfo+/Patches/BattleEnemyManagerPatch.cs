using Il2CppAssets.Scripts.GameCore.HostComponent;

namespace MDIP.Patches;

[HarmonyPatch(typeof(BattleEnemyManager), nameof(BattleEnemyManager.SetPlayResult))]
internal class BattleEnemyManagerSetPlayResultPatch
{
    private static void Postfix(int idx, byte result, bool isMulStart = false, bool isMulEnd = false, bool isLeft = false)
    {
        var note = GameStatsManager.GetMusicDataByIdx(idx);
        var type = (NoteType)note.noteData.type;

        if (Configs.Advanced.OutputNoteRecordsToDesktop)
            NoteRecordManager.AddRecord(idx, "SetPlayResult", $"result:{result}");

        switch (result)
        {
            case 4 when type == NoteType.Block:
                GameStatsManager.CountNote(idx, CountNoteAction.Block);
                break;
            case 1 when type == NoteType.Long:
                GameStatsManager.CountNote(idx, CountNoteAction.MissLong, -1, note.isLongPressStart);
                break;
        }

        if (type.IsRegularNote())
            GameStatsManager.UpdateCurrentSpeed(note.isAir, note.noteData.speed);
        GameStatsManager.UpdateCurrentStats();

        if (type == NoteType.Mul && result is 3 or 4)
            GameStatsManager.Mashing(note);
        else if (type.IsRegularNote() && !note.isLongPressing && type != NoteType.Mul)
        {
            GameStatsManager.CheckMashing(true);
            TextObjManager.UpdateAllText();
        }
    }
}

// Reference: https://github.com/flustix/AccDisplay/blob/main/AccDisplay/Patches/MissPatches.cs