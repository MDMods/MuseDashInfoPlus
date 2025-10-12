using JetBrains.Annotations;
using MDIP.Application.DependencyInjection;
using MDIP.Application.Services.Global.UI;
using MDIP.Application.Services.Scoped.UI;

namespace MDIP.Presentation.Patches;

[HarmonyPatch(typeof(PnlPreparation))]
internal static class PnlPreparationPatch
{
    [HarmonyPatch(nameof(PnlPreparation.OnDiffTglChanged))] [HarmonyPostfix]
    private static void OnDiffTglChangedPostfix(PnlPreparation __instance)
    {
        PreparationScreenService?.OnRecordUpdated(__instance);
    }

    [HarmonyPatch(nameof(PnlPreparation.OnBattleStart))] [HarmonyPrefix]
    private static void OnBattleStartPrefix(PnlPreparation __instance)
    {
        PreparationScreenService?.OnRecordUpdated(__instance);
    }

    [UsedImplicitly] [Inject] public static IPreparationScreenService PreparationScreenService { get; set; }
}