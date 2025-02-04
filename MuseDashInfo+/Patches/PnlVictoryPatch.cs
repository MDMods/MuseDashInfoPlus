using UnityEngine.UI;
using static MDIP.Managers.GameStatsManager;

namespace MDIP.Patches;

[HarmonyPatch(typeof(PnlVictory), nameof(PnlVictory.SetDetailInfo))]
internal class PnlVictorySetDetailInfoPatch
{
    private static void Postfix(PnlVictory __instance)
    {
        IsInGame = false;
        IsFirstTry = false;
        StoreHighestAccuracy((float)Math.Round(GetCalculatedAccuracy(), 2));
        StoreHighestScore(Current.Score);

        if (AccuracyRest != 0 || Math.Abs(Math.Round(GetTrueAccuracy(), 2) - Math.Round(GetCalculatedAccuracy(), 2)) > 0.0001f)
        {
            Melon<MDIPMod>.Logger.Error("===== Accuracy Calculation Error =====");
            Melon<MDIPMod>.Logger.Msg($"Total:{AccuracyTotal} | Counted:{AccuracyCounted} | Rest:{AccuracyRest}");
            Melon<MDIPMod>.Logger.Msg($"Total => Music:{Total.Music} | Energy:{Total.Energy} | Block:{Total.Block} | Hitable:{Total.Hittable}");
            Melon<MDIPMod>.Logger.Msg($"Counted => Music:{Current.Music} | Energy:{Current.Energy} | Block:{Current.Block} Perfect:{Current.Perfect} | Great:{Current.Great} /2f | | RedPoint:{Current.RedPoint}");
            Melon<MDIPMod>.Logger.Msg($"Miss => Music:{Miss.Music} | Energy:{Miss.Energy} | Block:{Miss.Block} | Hitable:{MissCountHittable} | LongPair:{Miss.LongPair}");
            Melon<MDIPMod>.Logger.Msg($"{AccuracyTotal} - {Current.Perfect + Current.Great + Current.Block + Current.Music + Current.Energy + Current.RedPoint} - {Miss.Music + Miss.Energy + MissCountHittable + Miss.LongPair + Miss.Block} = {AccuracyRest}");
            Melon<MDIPMod>.Logger.Error("======================================");
            Melon<MDIPMod>.Logger.Msg($"Calc Acc: {GetCalculatedAccuracy()} | True Acc:{GetTrueAccuracy()}");
            Melon<MDIPMod>.Logger.Error("======================================");
        }

        if (Configs.Advanced.OutputNoteRecordsToDesktop)
            NoteRecordManager.ExportToExcel();

        if (!Configs.Main.ReplaceResultsScreenMissCount) return;

        try
        {
            var pnlVictory = __instance.pnlVictory.GetComponentsInChildren<Transform>(true).FirstOrDefault(transform
                => transform.gameObject.activeSelf
                   && transform.name == "PnlVictory"
                   && transform.gameObject != __instance.pnlVictory);

            if (pnlVictory == null)
                throw new NullReferenceException("pnlVictory not found");

            pnlVictory.Find("PnlVictory_3D/DetailInfo/Other/TxtMiss/TxtValue")
                .gameObject.GetComponent<Text>()
                .text = (MissCountHittable + Miss.Block).ToString();
        }
        catch (Exception e)
        {
            Melon<MDIPMod>.Logger.Error(e.ToString());
        }
    }
}