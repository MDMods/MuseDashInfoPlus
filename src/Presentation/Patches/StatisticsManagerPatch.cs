using Il2CppAssets.Scripts.GameCore.Managers;
using MDIP.Battle;

namespace MDIP.Presentation.Patches;

[HarmonyPatch(typeof(StatisticsManager), nameof(StatisticsManager.OnNoteResult))]
internal static class StatisticsManagerPatch
{
    private static void Prefix(StatisticsManager __instance, int result)
        => BattleController.OnNoteResultRecord(result);
}
