using Il2Cpp;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

using MDIP.Manager;
using MDIP.Managers;

namespace MDIP.Patches;

[HarmonyPatch(typeof(PnlVictory), nameof(PnlVictory.SetDetailInfo))]
public class PnlVictorySetDetailInfoPatch
{
    private static void Postfix(PnlVictory __instance)
    {
        NoteRecordManager.ExportToExcel();

        if (!ConfigManager.ReplaceResultsScreenMissCount) return;

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
                        .text = (GameStatsManager.NormalMissCount + GameStatsManager.GhostMissCount).ToString();
                }
            }
        }
        catch (System.Exception e)
        {
            Melon<MDIPMod>.Logger.Error(e.ToString());
        }
    }
}