using UnityEngine.UI;

namespace MDIP.Managers;

public static class VictoryScreenManager
{
    public static void OnSetDetailInfo(PnlVictory instance)
    {
        GameStatsManager.IsInGame = false;
        GameStatsManager.IsFirstTry = false;

        var newAcc = (float)Math.Round(GameStatsManager.GetCalculatedAccuracy(), 2);
        var newScore = GameStatsManager.Current.Score;

        var newBest = Configs.Main.PersonalBestCriteria == 2
            ? newScore >= GameStatsManager.StoredHighestScore
            : newAcc >= GameStatsManager.StoredHighestAccuracy;

        GameStatsManager.StoreHighestAccuracy(newAcc);
        GameStatsManager.StoreHighestScore(newScore);

        if (newBest)
        {
            StatsSaverManager.SetStats(GameUtils.MusicHash,
                new()
                {
                    Great = GameStatsManager.Current.Great,
                    MissOther = GameStatsManager.MissCountHittable + GameStatsManager.Miss.Block,
                    MissCollectible = GameStatsManager.MissCountCollectible,
                    Early = GameStatsManager.Current.Early,
                    Late = GameStatsManager.Current.Late
                });
        }

        var trueAcc = Math.Round(GameStatsManager.GetTrueAccuracy(), 2);
        var calcAcc = Math.Round(GameStatsManager.GetCalculatedAccuracy(), 2);
        if (GameStatsManager.AccuracyCalculationRest != 0 || Math.Abs(trueAcc - calcAcc) > 0.0001f)
            GameStatsManager.OutputCurrentNoteDebuggingData(false);

        if (Configs.Advanced.OutputNoteRecordsToDesktop)
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Info+ Note Records");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            NoteRecordManager.ExportToExcel(Path.Combine(folder, $"{GameUtils.MusicName}.csv"));
        }

        if (!Configs.Main.ReplaceResultsScreenMissCount)
            return;

        try
        {
            var pnlVictory = instance.pnlVictory.GetComponentsInChildren<Transform>(true).FirstOrDefault(transform =>
                transform.gameObject.activeSelf &&
                transform.name == "PnlVictory" &&
                transform.gameObject != instance.pnlVictory);

            if (pnlVictory == null)
                throw new NullReferenceException("PnlVictory container not found.");

            pnlVictory.Find("PnlVictory_3D/DetailInfo/Other/TxtMiss/TxtValue")
                .gameObject.GetComponent<Text>()
                .text = (GameStatsManager.MissCountHittable + GameStatsManager.Miss.Block).ToString();
        }
        catch (Exception ex)
        {
            Melon<MDIPMod>.Logger.Error(ex.ToString());
        }
    }
}