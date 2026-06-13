using Il2CppFormulaBase;
using MDIP.Battle;

namespace MDIP.Presentation.Patches;

// The session starts on the SIMULATION's real start (StageBattleComponent.GameStart) — which fires
// once per run, post-countdown, downstream of the UI panel's PnlBattle.GameStart. Hooking it here
// (not the panel's GameStart) means Info+ never contends with mods that gate PnlBattle.GameStart: a
// multiplayer barrier that freezes the player at frame 0 suppresses the panel's GameStart, so this
// simply doesn't fire until the synced real start. No guard, no double-fire handling needed.
[HarmonyPatch(typeof(StageBattleComponent), "GameStart")]
internal static class StageBattleGameStartPatch
{
    private static void Postfix() => BattleController.OnBattleStart();
}
