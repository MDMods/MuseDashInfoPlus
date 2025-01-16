using Il2CppAssets.Scripts.GameCore.HostComponent;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppFormulaBase;
using Il2CppGameLogic;
using HarmonyLib;
using MelonLoader;

using MDIP.Manager;
using MDIP.Modules;
using MDIP.Managers;

namespace MDIP.Patches;

[HarmonyPatch(typeof(GameMissPlay), nameof(GameMissPlay.MissCube))]
public class GameMissPlayMissCubePatch
{
    private static void Postfix(int idx, decimal currentTick)
    {
        try
        {
            int result = Singleton<BattleEnemyManager>.instance.GetPlayResult(idx); // 0:Miss 1:Injuried 3:Great 4:Perfect
            var note = Singleton<StageBattleComponent>.instance.GetMusicDataByIdx(idx);
            var type = (NoteType)note.noteData.type;
            var isDouble = note.isDouble;

            NoteRecordManager.AddRecord(idx, "MissCube", $"result:{result}");

            if (result is 0 or 1)
            {
                switch (type)
                {
                    case NoteType.Ghost:
                        GameStatsManager.AddGhostMiss(idx);
                        break;

                    case NoteType.Heart:
                        GameStatsManager.AddHeartMiss(idx);
                        break;

                    case NoteType.Music:
                        GameStatsManager.AddMusicNoteMiss(idx);
                        break;

                    default:
                        if (result is 0 && type is NoteType.Block) break; // Missing an block does not count as a miss
                        if (result is 1 && isDouble) break; // Skip damaged double since there would always be a miss if this double should be counted
                        GameStatsManager.AddNormalMiss(idx, isDouble ? note.doubleIdx : -1);
                        break;
                }
            }

            StatsTextManager.UpdateAllText();
        }
        catch (System.Exception e)
        {
            Melon<MDIPMod>.Logger.Error(e.ToString());
        }
    }
}

// Reference: https://github.com/flustix/AccDisplay/blob/main/AccDisplay/Patches/MissPatches.cs