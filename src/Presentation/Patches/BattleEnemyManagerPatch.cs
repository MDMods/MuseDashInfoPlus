using Il2CppAssets.Scripts.GameCore.HostComponent;
using JetBrains.Annotations;
using MDIP.Application.DependencyInjection;
using MDIP.Application.Services.Scoped.Notes;

namespace MDIP.Presentation.Patches;

[HarmonyPatch(typeof(BattleEnemyManager), nameof(BattleEnemyManager.SetPlayResult))]
internal static class BattleEnemyManagerSetPlayResultPatch
{
    private static void Postfix(int idx, byte result, bool isMulStart = false, bool isMulEnd = false, bool isLeft = false)
    {
        NoteEventService.HandleSetPlayResult(idx, result, isMulStart, isMulEnd, isLeft);
    }

    [UsedImplicitly] [Inject] public static INoteEventService NoteEventService { get; set; }
}