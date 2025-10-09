using Il2CppAssets.Scripts.UI.Panels;
using JetBrains.Annotations;
using MDIP.Application.DependencyInjection;
using MDIP.Application.Services.UI;

namespace MDIP.Patches;

[HarmonyPatch(typeof(PnlBattle), nameof(PnlBattle.GameStart))]
internal static class PnlBattleGameStartPatch
{
    private static void Postfix(PnlBattle __instance)
    {
        BattleUIService.OnGameStart(__instance);
    }

    [UsedImplicitly] [Inject] public static IBattleUIService BattleUIService { get; set; }
}