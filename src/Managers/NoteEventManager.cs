using Il2CppAssets.Scripts.GameCore.HostComponent;

namespace MDIP.Managers;

public static class NoteEventManager
{
    public static void HandleSetPlayResult(int idx, byte result, bool isMulStart, bool isMulEnd, bool isLeft)
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

    public static void HandleMissCube(int idx, decimal currentTick)
    {
        try
        {
            var result = BattleEnemyManager.instance.GetPlayResult(idx);
            var note = GameStatsManager.GetMusicDataByIdx(idx);
            var type = (NoteType)note.noteData.type;
            var oid = note.objId;
            var isDouble = note.isDouble;

            if (Configs.Advanced.OutputNoteRecordsToDesktop)
                NoteRecordManager.AddRecord(note, "MissCube", $"missCube:{result}");

            if (result is 0 or 1)
            {
                switch (type)
                {
                    case NoteType.Ghost:
                        GameStatsManager.CountNote(oid, CountNoteAction.MissGhost);
                        break;
                    case NoteType.Energy:
                        GameStatsManager.CountNote(oid, CountNoteAction.MissEnergy);
                        break;
                    case NoteType.Music:
                        GameStatsManager.CountNote(oid, CountNoteAction.MissMusic);
                        break;
                    case NoteType.Block:
                        if (result != 0)
                            GameStatsManager.CountNote(oid, CountNoteAction.MissBlock);
                        break;
                    case NoteType.Mul:
                        break;
                    default:
                        GameStatsManager.CountNote(oid, CountNoteAction.MissMonster, isDouble ? GameStatsManager.GetMusicDataByIdx(note.doubleIdx).objId : (short)-1);
                        break;
                }
            }

            if (type == NoteType.Mul)
                GameStatsManager.CountMul(oid, result, (float)note.configData.length);

            if (!type.IsRegularNote() || type == NoteType.Block || note.isLongPressing)
                return;

            GameStatsManager.UpdateCurrentSpeed(note.isAir, note.noteData.speed);
            GameStatsManager.UpdateCurrentStats();
            TextObjManager.UpdateAllText();
        }
        catch (Exception ex)
        {
            Melon<MDIPMod>.Logger.Error(ex.ToString());
        }
    }
}