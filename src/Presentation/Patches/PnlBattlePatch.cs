using Il2CppAssets.Scripts.UI.Panels;
using MDIP.Battle;

namespace MDIP.Presentation.Patches;

// Build the overlay the instant the native battle UI appears — PnlBattle.GameStart's postfix, when
// the panel is set up and starting its own zoom-in. Info+'s text is parented under that panel, so it
// rides the game's built-in entrance for free.
//
// __runOriginal is false when another mod's prefix suppressed GameStart (a multiplayer barrier
// freezing frame 0); the native UI isn't set up then, so BattleController acts only when the original
// truly ran — the real start, where the panel + its zoom exist to ride.
[HarmonyPatch(typeof(PnlBattle), nameof(PnlBattle.GameStart))]
internal static class PnlBattleGameStartPatch
{
    private static void Postfix(PnlBattle __instance, bool __runOriginal)
        => BattleController.OnBattleStart(__instance, __runOriginal);
}
