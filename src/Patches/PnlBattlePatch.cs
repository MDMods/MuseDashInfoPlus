using Il2CppAssets.Scripts.UI.Panels;

namespace MDIP.Patches;

[HarmonyPatch(typeof(PnlBattle), nameof(PnlBattle.GameStart))]
internal class PnlBattleGameStartPatch
{
    private static void Postfix(PnlBattle __instance)
    {
        BattleUIManager.OnGameStart(__instance);
    }
}