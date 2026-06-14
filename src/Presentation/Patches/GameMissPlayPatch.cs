using Il2CppGameLogic;
using MDIP.Battle;

namespace MDIP.Presentation.Patches;

[HarmonyPatch(typeof(GameMissPlay), nameof(GameMissPlay.MissCube))]
internal static class GameMissPlayMissCubePatch
{
    private static void Postfix(int idx, decimal currentTick)
        => BattleController.OnMissCube(idx, currentTick);
}
