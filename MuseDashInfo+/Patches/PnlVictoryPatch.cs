using Il2Cpp;
using HarmonyLib;
using MelonLoader;
using System;

using MDIP.Manager;
using MDIP.Managers;
using static MDIP.Managers.GameStatsManager;

namespace MDIP.Patches;

[HarmonyPatch(typeof(PnlVictory), nameof(PnlVictory.SetDetailInfo))]
public class PnlVictorySetDetailInfoPatch
{
    private static void Postfix(PnlVictory __instance)
    {
        //NoteRecordManager.ExportToExcel();

        if (Math.Round(GetTrueAccuracy(), 2) != Math.Round(GetCalculatedAccuracy(), 2))
        {
            Melon<MDIPMod>.Logger.Msg($"======================================");
            Melon<MDIPMod>.Logger.Warning("The calculated accuracy is different from the actual accuracy");
            Melon<MDIPMod>.Logger.Msg($"Total:{MDAccTotal} | Counted:{MDAccCounted} | Rest:{MDAccRest}");
            Melon<MDIPMod>.Logger.Msg($"Total => TotalMusicCount:{TotalMusicCount} | TotalEnergyCount:{TotalEnergyCount} | TotalHitableCount:{TotalHitableCount} | TotalBlockCount:{TotalBlockCount}");
            Melon<MDIPMod>.Logger.Msg($"Counted => CurPerfectCount:{CurPerfectCount} | CurGreatCount:{CurGreatCount} /2f | CurBlockCount:{CurBlockCount} | CurMusicCount:{CurMusicCount} | CurEnergyCount:{CurEnergyCount} | CurRedPointCount:{CurRedPointCount}");
            Melon<MDIPMod>.Logger.Msg($"Miss => MissMusicCount:{MissMusicCount} | MissEnergyCount:{MissEnergyCount} | MissHitableCount:{MissHitableCount} | MissLongPair:{MissLongPairCount} | MissBlockCount:{MissBlockCount}");
            Melon<MDIPMod>.Logger.Msg($"{MDAccTotal} - {CurPerfectCount + CurGreatCount + CurBlockCount + CurMusicCount + CurEnergyCount + CurRedPointCount} - {MissMusicCount + MissEnergyCount + MissHitableCount - MissLongPairCount + MissBlockCount} = {MDAccRest}");
            Melon<MDIPMod>.Logger.Msg($"======================================");
            Melon<MDIPMod>.Logger.Warning($"Calc Acc: {GetCalculatedAccuracy()} | True Acc:{GetTrueAccuracy()}");
            Melon<MDIPMod>.Logger.Msg($"======================================");
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
                        .text = (MissCount).ToString();
                }
            }
        }
        catch (Exception e)
        {
            Melon<MDIPMod>.Logger.Error(e.ToString());
        }
    }
}