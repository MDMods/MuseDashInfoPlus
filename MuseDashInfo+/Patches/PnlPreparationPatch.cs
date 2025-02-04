namespace MDIP.Patches;

[HarmonyPatch(typeof(PnlPreparation))]
internal class PnlPreparationPatch
{
    [HarmonyPatch(nameof(PnlPreparation.OnDiffTglChanged))]
    [HarmonyPostfix]
    private static void OnDiffTglChangedPostfix(PnlPreparation __instance)
    {
        GameStatsManager.StoreHighestAccuracyFromText(__instance.pnlRecord.txtAccuracy?.m_Text);
        GameStatsManager.StoreHighestScoreFromText(__instance.pnlRecord.txtScore?.m_Text);
    }

    [HarmonyPatch(nameof(PnlPreparation.OnBattleStart))]
    [HarmonyPrefix]
    private static void OnBattleStartPrefix(PnlPreparation __instance)
    {
        GameStatsManager.StoreHighestAccuracyFromText(__instance.pnlRecord.txtAccuracy?.m_Text);
        GameStatsManager.StoreHighestScoreFromText(__instance.pnlRecord.txtScore?.m_Text);
    }
}