using Il2CppAssets.Scripts.UI.Panels;
using MDIP.Battle;

namespace MDIP.Presentation.Patches;

[HarmonyPatch(typeof(PnlBattle), nameof(PnlBattle.GameStart))]
internal static class PnlBattleGameStartPatch
{
    // __runOriginal is false when another mod's prefix suppressed GameStart (e.g. a multiplayer
    // barrier freezing the player at frame 0). BattleController only starts a session when the
    // original truly ran, and is idempotent, so the held re-invocation builds exactly one overlay.
    private static void Postfix(PnlBattle __instance, bool __runOriginal)
        => BattleController.OnGameStart(__instance, __runOriginal);
}
