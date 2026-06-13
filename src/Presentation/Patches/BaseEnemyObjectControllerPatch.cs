using MDIP.Battle;

namespace MDIP.Presentation.Patches;

[HarmonyPatch(typeof(BaseEnemyObjectController), nameof(BaseEnemyObjectController.ControllerMissCheck))]
internal static class BaseEnemyObjectControllerPatch
{
    private static void Postfix(BaseEnemyObjectController __instance)
        => BattleController.OnControllerMissRecord(__instance);
}
