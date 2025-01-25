using HarmonyLib;
using Il2CppAssets.Scripts.GameCore.HostComponent;
using Il2CppFormulaBase;
using Il2CppGameLogic;
using Il2CppSystem;
using MDIP.Managers;

namespace MDIP.Patches;

[HarmonyPatch(typeof(BattleRoleAttributeComponent), nameof(BattleRoleAttributeComponent.AttackScore))]
public class BattleRoleAttributeComponentAttackScorePatch
{
    private static void Prefix(BattleRoleAttributeComponent __instance, int idx, int result, TimeNodeOrder tno)
    {
        if (tno == null) return;

        Decimal d = (Decimal)GameGlobal.gTouch.tickTime - tno.md.tick;
        if (d > new Decimal(0.025)) GameStatsManager.AddLate(idx);
        else if (d < new Decimal(-0.025)) GameStatsManager.AddEarly(idx);
    }
}
