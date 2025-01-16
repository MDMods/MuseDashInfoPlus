using HarmonyLib;
using Il2Cpp;

using MDIP.Managers;

namespace MDIP.Patches;

[HarmonyPatch(typeof(PnlPreparation))]
public class PnlPreparationPatch
{
    private static void SetHighestScore(string score)
    {
        if (!string.IsNullOrEmpty(score))
        {
            if (score == "-")
                GameStatsManager.SavedHighestScore = 0;
            else if (int.TryParse(score, out var x) && x >= 1)
                GameStatsManager.SavedHighestScore = int.Parse(score);
        }
    }

    [HarmonyPatch(nameof(PnlPreparation.OnDiffTglChanged)), HarmonyPostfix]
    private static void OnDiffTglChangedPostfix(PnlPreparation __instance)
        => SetHighestScore(__instance.pnlRecord.txtScore?.text);

    [HarmonyPatch(nameof(PnlPreparation.OnBattleStart)), HarmonyPrefix]
    private static void OnBattleStartPrefix(PnlPreparation __instance)
    {
        SetHighestScore(__instance.pnlRecord.txtScore?.text);
        GameStatsManager.LockHighestScore();
    }
}