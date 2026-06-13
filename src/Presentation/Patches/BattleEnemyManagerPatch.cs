using Il2CppAssets.Scripts.GameCore.HostComponent;
using MDIP.Battle;

namespace MDIP.Presentation.Patches;

[HarmonyPatch(typeof(BattleEnemyManager), nameof(BattleEnemyManager.SetPlayResult))]
internal static class BattleEnemyManagerSetPlayResultPatch
{
    private static void Postfix(int idx, byte result, bool isMulStart = false, bool isMulEnd = false, bool isLeft = false)
        => BattleController.OnSetPlayResult(idx, result, isMulStart, isMulEnd, isLeft);
}
