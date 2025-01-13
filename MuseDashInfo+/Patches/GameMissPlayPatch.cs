using Il2CppAssets.Scripts.GameCore.HostComponent;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppFormulaBase;
using Il2CppGameLogic;
using HarmonyLib;
using MelonLoader;

using MuseDashInfoPlus.Utils;
using MuseDashInfoPlus.Manager;

namespace MuseDashInfoPlus.Patches;

[HarmonyPatch(typeof(GameMissPlay), nameof(GameMissPlay.MissCube))]
public class GameMissPlayMissCubePatch
{
    private static void Postfix(int idx, decimal currentTick)
    {
        try
        {
            int lane = Singleton<BattleEnemyManager>.instance.GetPlayResult(idx);
            var noteData = Singleton<StageBattleComponent>.instance.GetMusicDataByIdx(idx);
            var noteType = noteData.noteData.type;
            var isDouble = noteData.isDouble;

            switch (lane)
            {
                case 0: // Air
                    switch (noteType)
                    {
                        case 4:
                            Utils.GameStatsUtils.GhostMissCount++;
                            break;

                        case 6 or 7:
                            Utils.GameStatsUtils.CollectableMissCount++;
                            break;

                        default:
                            if (noteType != 2 && !isDouble)
                                Utils.GameStatsUtils.NormalMissCount++;
                            break;
                    }
                    break;

                case 1: // Ground
                    switch (noteType)
                    {
                        case 4:
                            Utils.GameStatsUtils.GhostMissCount++;
                            break;

                        case 6 or 7:
                            Utils.GameStatsUtils.CollectableMissCount++;
                            break;

                        default:
                            Utils.GameStatsUtils.NormalMissCount++;
                            break;
                    }
                    break;
            }

            CountsTextManager.UpdatePlusCountsText();
        }
        catch (System.Exception e)
        {
            Melon<InfoPlusMod>.Logger.Error(e.ToString());
        }
    }
}

// Reference: https://github.com/flustix/AccDisplay/blob/main/AccDisplay/Patches/MissPatches.cs