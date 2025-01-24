using Il2CppAssets.Scripts.GameCore.HostComponent;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppFormulaBase;
using Il2CppGameLogic;
using HarmonyLib;
using MelonLoader;

using MDIP.Managers;
using MDIP.Modules;
using MDIP.Utils;

namespace MDIP.Patches;

[HarmonyPatch(typeof(GameMissPlay), nameof(GameMissPlay.MissCube))]
public class GameMissPlayMissCubePatch
{
    private static void Postfix(int idx, decimal currentTick)
    {
        try
        {
            int result = Singleton<BattleEnemyManager>.instance.GetPlayResult(idx);
            var note = Singleton<StageBattleComponent>.instance.GetMusicDataByIdx(idx);
            var type = (NoteType)note.noteData.type;
            var isDouble = note.isDouble;

            if (Utils.Helper.OutputNoteRecordsToDesktop)
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

                    default:
                        GameStatsManager.CountNote(idx, CountNoteAction.MissMonster, isDouble ? note.doubleIdx : -1);
                        break;
                }
            }

            if (type.IsRegularNote() && type != NoteType.Block && !note.isLongPressing)
            {
                GameStatsManager.CheckMashing();
                TextObjManager.UpdateAllText();
            }
        }
        catch (System.Exception e)
        {
            Melon<MDIPMod>.Logger.Error(e.ToString());
        }
    }
}

// Reference: https://github.com/flustix/AccDisplay/blob/main/AccDisplay/Patches/MissPatches.cs