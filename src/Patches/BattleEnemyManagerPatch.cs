using Il2CppAssets.Scripts.GameCore.HostComponent;
using JetBrains.Annotations;
using MDIP.Application.Contracts;

namespace MDIP.Patches;

[HarmonyPatch(typeof(BattleEnemyManager), nameof(BattleEnemyManager.SetPlayResult))]
internal static class BattleEnemyManagerSetPlayResultPatch
{
    private static void Postfix(int idx, byte result, bool isMulStart = false, bool isMulEnd = false, bool isLeft = false)
    {
        NoteEventService.HandleSetPlayResult(idx, result, isMulStart, isMulEnd, isLeft);
    }

    #region Injections

    [UsedImplicitly] public static INoteEventService NoteEventService { get; set; }

    #endregion
}