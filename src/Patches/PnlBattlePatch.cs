using Il2CppAssets.Scripts.UI.Panels;
using JetBrains.Annotations;
using MDIP.Application.Contracts;

namespace MDIP.Patches;

[HarmonyPatch(typeof(PnlBattle), nameof(PnlBattle.GameStart))]
internal static class PnlBattleGameStartPatch
{
    private static void Postfix(PnlBattle __instance)
    {
        BattleUIService.OnGameStart(__instance);
    }

    #region Injections

    [UsedImplicitly] public static IBattleUIService BattleUIService { get; set; }

    #endregion
}