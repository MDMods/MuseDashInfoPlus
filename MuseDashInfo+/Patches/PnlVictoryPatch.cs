using Il2Cpp;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace MuseDashInfoPlus.Patches;

[HarmonyPatch(typeof(PnlVictory), nameof(PnlVictory.SetDetailInfo))]
public class PnlVictorySetDetailInfoPatch
{
    private static void Postfix(PnlVictory __instance)
    {
        try
        {
            var transforms = __instance.pnlVictory.GetComponentsInChildren<Transform>(true);

            foreach (var transform in transforms)
            {
                if (transform.gameObject.activeSelf &&
                    transform.name == "PnlVictory" &&
                    transform.gameObject != __instance.pnlVictory)
                {
                    transform.Find("PnlVictory_3D/DetailInfo/Other/TxtMiss/TxtValue")
                        .gameObject.GetComponent<UnityEngine.UI.Text>()
                        .text = Utils.GameStatsUtils.MissCount.ToString();
                }
            }
        }
        catch (System.Exception e)
        {
            Melon<InfoPlusMod>.Logger.Error(e.ToString());
        }
    }
}