using Il2CppAssets.Scripts.GameCore.HostComponent;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppFormulaBase;
using HarmonyLib;

namespace MuseDashInfoPlus.Patches;

[HarmonyPatch(typeof(BattleEnemyManager), nameof(BattleEnemyManager.SetPlayResult))]
public class BattleEnemyManagerSetPlayResultPatch
{
    private static void Postfix(int idx, byte result, bool isMulStart = false, bool isMulEnd = false, bool isLeft = false)
    {
        var noteData = Singleton<StageBattleComponent>.instance.GetMusicDataByIdx(idx);
        var noteType = noteData.noteData.type;

        switch (result)
        {
            case 4 when noteType == 2:
                Utils.GameStatsUtils.JumpOverCount++;
                break;
            case 1 when noteType == 3:
                Utils.GameStatsUtils.NormalMissCount++;
                break;
        }
    }
}

// Reference: https://github.com/flustix/AccDisplay/blob/main/AccDisplay/Patches/MissPatches.cs