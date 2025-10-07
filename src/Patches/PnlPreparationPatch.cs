namespace MDIP.Patches;

[HarmonyPatch(typeof(PnlPreparation))]
internal class PnlPreparationPatch
{
    [HarmonyPatch(nameof(PnlPreparation.OnDiffTglChanged))]
    [HarmonyPostfix]
    private static void OnDiffTglChangedPostfix(PnlPreparation __instance)
    {
        PreparationScreenManager.OnRecordUpdated(__instance);
    }

    [HarmonyPatch(nameof(PnlPreparation.OnBattleStart))]
    [HarmonyPrefix]
    private static void OnBattleStartPrefix(PnlPreparation __instance)
    {
        PreparationScreenManager.OnRecordUpdated(__instance);
    }
}