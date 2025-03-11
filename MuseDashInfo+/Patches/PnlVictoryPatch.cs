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

        var newAcc = (float)Math.Round(GetCalculatedAccuracy(), 2);
        var newScore = Current.Score;

        var newBest = Configs.Main.PersonalBestCriteria == 2
            ? newScore >= StoredHighestScore
            : newAcc >= StoredHighestAccuracy;

        StoreHighestAccuracy(newAcc);
        StoreHighestScore(newScore);

        if (newBest)
        {
            StatsSaverManager.SetStats(GameUtils.MusicHash,
                new()
                {
                    Great = Current.Great,
                    MissOther = MissCountHittable + Miss.Block,
                    MissCollectible = MissCountCollectible,
                    Early = Current.Early,
                    Late = Current.Late
                });
        }

        if (AccuracyCalculationRest != 0 || Math.Abs(Math.Round(GetTrueAccuracy(), 2) - Math.Round(GetCalculatedAccuracy(), 2)) > 0.0001f)
            OutputCurrentNoteDebuggingData(false);

        if (Configs.Advanced.OutputNoteRecordsToDesktop)
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Info+ Note Records\";
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            NoteRecordManager.ExportToExcel($"{folder}{GameUtils.MusicName} Note Records.csv");
        }

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