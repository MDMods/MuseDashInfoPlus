using Il2CppAssets.Scripts.GameCore.HostComponent;

namespace MDIP.Patches;

[HarmonyPatch(typeof(BattleEnemyManager), nameof(BattleEnemyManager.SetPlayResult))]
internal class BattleEnemyManagerSetPlayResultPatch
{
    private static void Postfix(int idx, byte result, bool isMulStart = false, bool isMulEnd = false, bool isLeft = false)
    {
        NoteEventManager.HandleSetPlayResult(idx, result, isMulStart, isMulEnd, isLeft);
    }
}