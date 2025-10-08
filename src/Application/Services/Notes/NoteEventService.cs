using Il2CppAssets.Scripts.GameCore.HostComponent;
using JetBrains.Annotations;
using MDIP.Application.Contracts;
using MDIP.Domain.Enums;
using MDIP.Utils;

namespace MDIP.Application.Services.Notes;

public class NoteEventService : INoteEventService
{
    public void HandleSetPlayResult(int idx, byte result, bool isMulStart, bool isMulEnd, bool isLeft)
    {
        var note = GameStatsService.GetMusicDataByIdx(idx);
        var type = (NoteType)note.noteData.type;
        var oid = note.objId;

        if (ConfigAccessor.Advanced.OutputNoteRecordsToDesktop)
            NoteRecordService.AddRecord(note, "SetPlayResult", $"setResult:{result}");

        switch (result)
        {
            case 4 when type == NoteType.Block:
                GameStatsService.CountNote(oid, CountNoteAction.Block);
                break;
            case 1 when type == NoteType.Long:
                GameStatsService.CountNote(oid, CountNoteAction.MissLong, -1, note.isLongPressStart);
                break;
        }

        if (type == NoteType.Mul)
            GameStatsService.CountMul(oid, result, (float)note.configData.length);

        if (!type.IsRegularNote() || note.isLongPressing)
            return;

        GameStatsService.UpdateCurrentSpeed(note.isAir, note.noteData.speed);
        GameStatsService.UpdateCurrentStats();
    }

    public void HandleMissCube(int idx, decimal currentTick)
    {
        try
        {
            var result = BattleEnemyManager.instance.GetPlayResult(idx);
            var note = GameStatsService.GetMusicDataByIdx(idx);
            var type = (NoteType)note.noteData.type;
            var oid = note.objId;
            var isDouble = note.isDouble;

            if (ConfigAccessor.Advanced.OutputNoteRecordsToDesktop)
                NoteRecordService.AddRecord(note, "MissCube", $"missCube:{result}");

            if (result is 0 or 1)
            {
                switch (type)
                {
                    case NoteType.Ghost:
                        GameStatsService.CountNote(oid, CountNoteAction.MissGhost);
                        break;
                    case NoteType.Energy:
                        GameStatsService.CountNote(oid, CountNoteAction.MissEnergy);
                        break;
                    case NoteType.Music:
                        GameStatsService.CountNote(oid, CountNoteAction.MissMusic);
                        break;
                    case NoteType.Block:
                        if (result != 0)
                            GameStatsService.CountNote(oid, CountNoteAction.MissBlock);
                        break;
                    case NoteType.Mul:
                        break;
                    default:
                        GameStatsService.CountNote(oid, CountNoteAction.MissMonster, isDouble ? GameStatsService.GetMusicDataByIdx(note.doubleIdx).objId : (short)-1);
                        break;
                }
            }

            Melon<MDIPMod>.Logger.Msg("");
            Melon<MDIPMod>.Logger.Error("");
            Melon<MDIPMod>.Logger.BigError("");
            Melon<MDIPMod>.Logger.Warning("");

            if (type == NoteType.Mul)
                GameStatsService.CountMul(oid, result, (float)note.configData.length);

            if (!type.IsRegularNote() || type == NoteType.Block || note.isLongPressing)
                return;

            GameStatsService.UpdateCurrentSpeed(note.isAir, note.noteData.speed);
            GameStatsService.UpdateCurrentStats();
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }

    #region Injections

    [UsedImplicitly] public required IConfigAccessor ConfigAccessor { get; init; }
    [UsedImplicitly] public required IGameStatsService GameStatsService { get; init; }
    [UsedImplicitly] public required INoteRecordService NoteRecordService { get; init; }
    [UsedImplicitly] public required ILogger<NoteEventService> Logger { get; init; }

    #endregion
}