namespace MDIP.Patches;

[HarmonyPatch(typeof(PnlPreparation))]
internal class PnlPreparationPatch
{
    private static void SetHighestAccuracy(string accuracy)
    {
        if (string.IsNullOrEmpty(accuracy))
            return;

        GameStatsManager.IsFirstTry = accuracy == "-";
        if (GameStatsManager.IsFirstTry)
            GameStatsManager.PrepPageAccuracy = 0;
        else if (float.TryParse(accuracy.TrimEnd(' ', '%'), out var x) && x > 0)
            GameStatsManager.PrepPageAccuracy = x;
    }

    private static void SetHighestScore(string score)
    {
        if (string.IsNullOrEmpty(score))
            return;

        GameStatsManager.IsFirstTry = score == "-";
        if (GameStatsManager.IsFirstTry)
            GameStatsManager.PrepPageScore = 0;
        else if (int.TryParse(score, out var x) && x >= 1)
            GameStatsManager.PrepPageScore = x;
    }

    [HarmonyPatch(nameof(PnlPreparation.OnDiffTglChanged))]
    [HarmonyPostfix]
    private static void OnDiffTglChangedPostfix(PnlPreparation __instance)
    {
        SetHighestAccuracy(__instance.pnlRecord.txtAccuracy?.m_Text);
        SetHighestScore(__instance.pnlRecord.txtScore?.m_Text);
    }

    [HarmonyPatch(nameof(PnlPreparation.OnBattleStart))]
    [HarmonyPrefix]
    private static void OnBattleStartPrefix(PnlPreparation __instance)
    {
        SetHighestAccuracy(__instance.pnlRecord.txtAccuracy?.m_Text);
        SetHighestScore(__instance.pnlRecord.txtScore?.m_Text);
    }
}