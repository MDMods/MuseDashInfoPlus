using JetBrains.Annotations;
using MDIP.Application.Contracts;

namespace MDIP.Patches;

[HarmonyPatch(typeof(PnlVictory), nameof(PnlVictory.SetDetailInfo))]
internal static class PnlVictorySetDetailInfoPatch
{
    private static void Postfix(PnlVictory __instance)
    {
        VictoryScreenService.OnSetDetailInfo(__instance);
    }

    #region Injections

    [UsedImplicitly] public static IVictoryScreenService VictoryScreenService { get; set; }

    #endregion
}