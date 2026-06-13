using Il2CppAssets.Scripts.GameCore.HostComponent;
using MDIP.Core.Domain.Enums;
using MDIP.Core.Utilities;
using MDIP.Globals;

namespace MDIP.Battle;

// Translates the game's per-note result callbacks into GameStats counts. Owned by a BattleSession;
// drives its sibling GameStats and (when debugging) NoteRecords.
public class NoteEvents(GameStats stats, NoteRecords records)
{
    public void HandleSetPlayResult(int idx, byte result, bool isMulStart, bool isMulEnd, bool isLeft)
    {
        var note = stats.GetMusicDataByIdx(idx);
        var type = (NoteType)note.noteData.type;
        var oid = note.objId;

        if (Config.Advanced.OutputNoteRecordsToDesktop)
            records.AddRecord(note, "SetPlayResult", $"setResult:{result}");

        switch (result)
        {
            case 4 when type == NoteType.Block:
                stats.CountNote(oid, CountNoteAction.Block);
                break;
            case 1 when type == NoteType.Long:
                stats.CountNote(oid, CountNoteAction.MissLong, -1, note.isLongPressStart);
                break;
        }

        if (type == NoteType.Mul)
            stats.CountMul(oid, result, (float)note.configData.length);

        if (!type.IsRegularNote() || note.isLongPressing)
            return;

        stats.UpdateCurrentSpeed(note.isAir, note.noteData.speed);
        stats.UpdateCurrentStats();
    }

    public void HandleMissCube(int idx, decimal currentTick)
    {
        try
        {
            var result = BattleEnemyManager.instance.GetPlayResult(idx);
            var note = stats.GetMusicDataByIdx(idx);
            var type = (NoteType)note.noteData.type;
            var oid = note.objId;
            var isDouble = note.isDouble;

            if (Config.Advanced.OutputNoteRecordsToDesktop)
                records.AddRecord(note, "MissCube", $"missCube:{result}");

            if (result is 0 or 1)
            {
                switch (type)
                {
                    case NoteType.Ghost:
                        stats.CountNote(oid, CountNoteAction.MissGhost);
                        break;
                    case NoteType.Energy:
                        stats.CountNote(oid, CountNoteAction.MissEnergy);
                        break;
                    case NoteType.Music:
                        stats.CountNote(oid, CountNoteAction.MissMusic);
                        break;
                    case NoteType.Block:
                        if (result != 0)
                            stats.CountNote(oid, CountNoteAction.MissBlock);
                        break;
                    case NoteType.Mul:
                        break;
                    default:
                        stats.CountNote(oid, CountNoteAction.MissMonster, isDouble ? stats.GetMusicDataByIdx(note.doubleIdx).objId : (short)-1);
                        break;
                }
            }

            if (type == NoteType.Mul)
                stats.CountMul(oid, result, (float)note.configData.length);

            if (!type.IsRegularNote() || type == NoteType.Block || note.isLongPressing)
                return;

            stats.UpdateCurrentSpeed(note.isAir, note.noteData.speed);
            stats.UpdateCurrentStats();
        }
        catch (Exception ex)
        {
            Log.Error("Handle miss note (HandleMissCube) failed.");
            Log.Error(ex);
        }
    }
}
