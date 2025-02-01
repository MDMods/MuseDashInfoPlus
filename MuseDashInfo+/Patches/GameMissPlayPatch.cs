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
            var isDouble = note.isDouble;

            if (Configs.Advanced.OutputNoteRecordsToDesktop)
                NoteRecordManager.AddRecord(idx, "MissCube", $"result:{result}");

            if (result is 0 or 1)
            {
                switch (type)
                {
                    case NoteType.Ghost:
                        GameStatsManager.CountNote(idx, CountNoteAction.MissGhost);
                        break;

                    case NoteType.Energy:
                        GameStatsManager.CountNote(idx, CountNoteAction.MissEnergy);
                        break;

                    case NoteType.Music:
                        GameStatsManager.CountNote(idx, CountNoteAction.MissMusic);
                        break;

                    case NoteType.Block:
                        if (result != 0)
                            GameStatsManager.CountNote(idx, CountNoteAction.MissBlock);
                        break;

                    case NoteType.Mul:
                        GameStatsManager.CountNote(idx, CountNoteAction.MissMul);
                        break;

                    case NoteType.Monster:
                    case NoteType.Long:
                    case NoteType.Boss:
                    default:
                        GameStatsManager.CountNote(idx, CountNoteAction.MissMonster, isDouble ? note.doubleIdx : -1);
                        break;
                }
            }

            if (!type.IsRegularNote() || type == NoteType.Block || note.isLongPressing)
                return;

            GameStatsManager.UpdateCurrentSpeed(note.isAir, note.noteData.speed);
            GameStatsManager.UpdateCurrentStats();
            GameStatsManager.CheckMashing();
            TextObjManager.UpdateAllText();
        }
        catch (Exception e)
        {
            Melon<MDIPMod>.Logger.Error(e.ToString());
        }
    }
}

// Reference: https://github.com/flustix/AccDisplay/blob/main/AccDisplay/Patches/MissPatches.cs