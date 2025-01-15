using Il2CppAssets.Scripts.GameCore.HostComponent;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppFormulaBase;
using HarmonyLib;
using MDIP.Modules;

namespace MDIP.Patches;

[HarmonyPatch(typeof(BattleEnemyManager), nameof(BattleEnemyManager.SetPlayResult))]
public class BattleEnemyManagerSetPlayResultPatch
{
    private static void Postfix(int idx, byte result, bool isMulStart = false, bool isMulEnd = false, bool isLeft = false)
    {
        var noteData = Singleton<StageBattleComponent>.instance.GetMusicDataByIdx(idx);
        var noteType = (NoteType)noteData.noteData.type;

        switch (result)
        {
            case 4 when noteType == NoteType.Block:
                Utils.GameStatsUtils.JumpOverCount++;
                break;
            case 1 when noteType == NoteType.Long:
                Utils.GameStatsUtils.NormalMissCount++;
                break;
        }
    }
}

// Reference: https://github.com/flustix/AccDisplay/blob/main/AccDisplay/Patches/MissPatches.cs