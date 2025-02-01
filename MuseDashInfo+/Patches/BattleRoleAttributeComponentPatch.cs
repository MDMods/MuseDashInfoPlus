using HarmonyLib;
using Il2CppAssets.Scripts.GameCore.HostComponent;
using Il2CppAssets.Scripts.PeroTools.Commons;
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
		var md = Singleton<StageBattleComponent>.instance.GetMusicDataByIdx(idx);
		if (md.isLongPressing || md.isMul || tno == null) return;

		var d = (Decimal)GameGlobal.gTouch.tickTime - tno.md.tick;
		if (d > new Decimal(0.025)) GameStatsManager.AddLate();
		else if (d < new Decimal(-0.025)) GameStatsManager.AddEarly();
	}
}