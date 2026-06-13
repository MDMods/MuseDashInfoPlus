using MDIP.Battle;

namespace MDIP.Presentation.Patches;

[HarmonyPatch(typeof(PnlVictory), nameof(PnlVictory.SetDetailInfo))]
internal static class PnlVictorySetDetailInfoPatch
{
    private static void Postfix(PnlVictory __instance)
        => BattleController.OnVictoryDetail(__instance);
}
