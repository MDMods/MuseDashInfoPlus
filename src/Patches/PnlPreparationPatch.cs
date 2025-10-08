using JetBrains.Annotations;
using MDIP.Application.Contracts;

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

    #region Injections

    [UsedImplicitly] public static IPreparationScreenService PreparationScreenService { get; set; }

    #endregion
}