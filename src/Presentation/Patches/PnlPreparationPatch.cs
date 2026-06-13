using MDIP.Battle;

namespace MDIP.Presentation.Patches;

[HarmonyPatch(typeof(PnlPreparation))]
internal static class PnlPreparationPatch
{
    [HarmonyPatch(nameof(PnlPreparation.OnDiffTglChanged))] [HarmonyPostfix]
    private static void OnDiffTglChangedPostfix(PnlPreparation __instance)
        => BattleController.OnPrepRecordUpdated(__instance);

    [HarmonyPatch(nameof(PnlPreparation.OnBattleStart))] [HarmonyPrefix]
    private static void OnBattleStartPrefix(PnlPreparation __instance)
        => BattleController.OnPrepRecordUpdated(__instance);
}
