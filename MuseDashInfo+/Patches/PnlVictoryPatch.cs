using Il2Cpp;
using HarmonyLib;
using MelonLoader;
using System;

using MDIP.Manager;

using static MDIP.Managers.GameStatsManager;

namespace MDIP.Patches;

[HarmonyPatch(typeof(PnlVictory), nameof(PnlVictory.SetDetailInfo))]
public class PnlVictorySetDetailInfoPatch
{
    private static void Postfix(PnlVictory __instance)
    {
        //NoteRecordManager.ExportToExcel();

        if (AccuracyRest != 0 || Math.Round(GetTrueAccuracy(), 2) != Math.Round(GetCalculatedAccuracy(), 2))
        {
            Melon<MDIPMod>.Logger.Error($"===== Accuracy Calculation Error =====");
            Melon<MDIPMod>.Logger.Msg($"Total:{AccuracyTotal} | Counted:{AccuracyCounted} | Rest:{AccuracyRest}");
            Melon<MDIPMod>.Logger.Msg($"Total => Music:{Total.Music} | Energy:{Total.Energy} | Block:{Total.Block} | Hitable:{Total.Hitable}");
            Melon<MDIPMod>.Logger.Msg($"Counted => Music:{Current.Music} | Energy:{Current.Energy} | Block:{Current.Block} Perfect:{Current.Perfect} | Great:{Current.Great} /2f | | RedPoint:{Current.RedPoint}");
            Melon<MDIPMod>.Logger.Msg($"Miss => Music:{Miss.Music} | Energy:{Miss.Energy} | Block:{Miss.Block} | Hitable:{MissCountHitable} | LongPair:{Miss.LongPair}");
            Melon<MDIPMod>.Logger.Msg($"{AccuracyTotal} - {Current.Perfect + Current.Great + Current.Block + Current.Music + Current.Energy + Current.RedPoint} - {Miss.Music + Miss.Energy + MissCountHitable + Miss.LongPair + Miss.Block} = {AccuracyRest}");
            Melon<MDIPMod>.Logger.Error($"======================================");
            Melon<MDIPMod>.Logger.Msg($"Calc Acc: {GetCalculatedAccuracy()} | True Acc:{GetTrueAccuracy()}");
            Melon<MDIPMod>.Logger.Error($"======================================");
        }

        if (!ConfigManager.ReplaceResultsScreenMissCount) return;

        try
        {
            var transforms = __instance.pnlVictory.GetComponentsInChildren<UnityEngine.Transform>(true);

            foreach (var transform in transforms)
            {
                if (transform.gameObject.activeSelf &&
                    transform.name == "PnlVictory" &&
                    transform.gameObject != __instance.pnlVictory)
                {
                    transform.Find("PnlVictory_3D/DetailInfo/Other/TxtMiss/TxtValue")
                        .gameObject.GetComponent<UnityEngine.UI.Text>()
                        .text = (MissCountHitable + Miss.Block).ToString();
                }
            }
        }
        catch (Exception e)
        {
            Melon<MDIPMod>.Logger.Error(e.ToString());
        }
    }
}