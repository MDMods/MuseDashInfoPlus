using JetBrains.Annotations;
using MDIP.Application.Services.UI;

namespace MDIP.Patches;

[HarmonyPatch(typeof(PnlPreparation))]
internal static class PnlPreparationPatch
{
    [HarmonyPatch(nameof(PnlPreparation.OnDiffTglChanged))]
    [HarmonyPostfix]
    private static void OnDiffTglChangedPostfix(PnlPreparation __instance)
    {
        PreparationScreenService.OnRecordUpdated(__instance);
    }

    [HarmonyPatch(nameof(PnlPreparation.OnBattleStart))]
    [HarmonyPrefix]
    private static void OnBattleStartPrefix(PnlPreparation __instance)
    {
        PreparationScreenService.OnRecordUpdated(__instance);
    }

    [UsedImplicitly] public static IPreparationScreenService PreparationScreenService { get; set; }
}