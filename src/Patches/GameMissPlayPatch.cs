using Il2CppAssets.Scripts.GameCore.HostComponent;
using Il2CppGameLogic;

namespace MDIP.Patches;

[HarmonyPatch(typeof(GameMissPlay), nameof(GameMissPlay.MissCube))]
internal class GameMissPlayMissCubePatch
{
    private static void Postfix(int idx, decimal currentTick)
    {
        try
        {
            int result = BattleEnemyManager.instance.GetPlayResult(idx);
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

                    case NoteType.Monster:
                    case NoteType.Long:
                    case NoteType.Boss:
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
        catch (Exception e)
        {
            Melon<MDIPMod>.Logger.Error(e.ToString());
        }
    }
}

// Reference: https://github.com/flustix/AccDisplay/blob/main/AccDisplay/Patches/MissPatches.cs