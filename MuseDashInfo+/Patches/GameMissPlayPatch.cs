using Il2CppAssets.Scripts.GameCore.HostComponent;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppFormulaBase;
using Il2CppGameLogic;
using HarmonyLib;
using MelonLoader;

using MDIP.Manager;
using MDIP.Modules;

namespace MDIP.Patches;

[HarmonyPatch(typeof(GameMissPlay), nameof(GameMissPlay.MissCube))]
public class GameMissPlayMissCubePatch
{
    private static void Postfix(int idx, decimal currentTick)
    {
        try
        {
            int result = Singleton<BattleEnemyManager>.instance.GetPlayResult(idx); // 0:Miss 1:Injuried 3:Great 4:Perfect
            var noteData = Singleton<StageBattleComponent>.instance.GetMusicDataByIdx(idx);
            var noteType = (NoteType)noteData.noteData.type;
            var isDouble = noteData.isDouble;

            switch (result)
            {
                case 0: // Miss
                    switch (noteType)
                    {
                        case NoteType.Ghost:
                            Utils.GameStatsUtils.GhostMissCount++;
                            break;

                        case NoteType.Heart or NoteType.Music:
                            Utils.GameStatsUtils.CollectableMissCount++;
                            break;

                        default:
                            if (noteType != NoteType.Block && !isDouble)
                                Utils.GameStatsUtils.NormalMissCount++;
                            break;
                    }
                    break;

                case 1: // Injuried
                    switch (noteType)
                    {
                        case NoteType.Ghost:
                            Utils.GameStatsUtils.GhostMissCount++;
                            break;

                        case NoteType.Heart or NoteType.Music:
                            Utils.GameStatsUtils.CollectableMissCount++;
                            break;

                        default:
                            Utils.GameStatsUtils.NormalMissCount++;
                            break;
                    }
                    break;
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