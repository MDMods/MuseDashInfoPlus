using MDIP.Battle;

namespace MDIP.Presentation.Patches;

[HarmonyPatch(typeof(MultHitEnemyController), nameof(MultHitEnemyController.OnControllerMiss))]
internal static class MultHitEnemyControllerPatch
{
    private static void Prefix(MultHitEnemyController __instance, int index)
        => BattleController.OnMultHitMissRecord(__instance);
}
