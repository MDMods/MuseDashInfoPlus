using JetBrains.Annotations;
using MDIP.Application.DependencyInjection;
using MDIP.Application.Services.Scoped.UI;

namespace MDIP.Presentation.Patches;

[HarmonyPatch(typeof(PnlVictory), nameof(PnlVictory.SetDetailInfo))]
internal static class PnlVictorySetDetailInfoPatch
{
    private static void Postfix(PnlVictory __instance)
    {
        VictoryScreenService?.OnSetDetailInfo(__instance);
    }

    [UsedImplicitly] [Inject] public static IVictoryScreenService VictoryScreenService { get; set; }
}