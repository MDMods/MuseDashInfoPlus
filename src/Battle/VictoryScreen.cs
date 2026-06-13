using MDIP.Core.Utilities;
using MDIP.Globals;
using MDIP.Presentation;

namespace MDIP.Battle;

// Handles the results screen: persists the run as a new personal best when it beats the stored one,
// optionally exports note records, and (optionally) replaces the native miss count with our hittable
// count. Owned by a BattleSession; reads its siblings GameStats and NoteRecords.
public class VictoryScreen(GameStats stats, NoteRecords records)
{
    public void OnSetDetailInfo(PnlVictory instance)
    {
        stats.IsPlayerPlaying = false;

        var hash = stats.PlayingMusicHash;

        var trueAccuracy = Math.Round(stats.GetTrueAccuracy(), 2);
        var newAcc = (float)trueAccuracy;
        var newScore = stats.Current.Score;

        var accBest = RuntimeData.StorePersonalBestAccuracy(hash, newAcc);
        var scoreBest = RuntimeData.StorePersonalBestScore(hash, newScore);

        var mainConfig = Config.Main;
        var newBest = mainConfig.PersonalBestCriteria == 2 ? scoreBest : accBest;

        if (newBest)
        {
            StatsStore.SetStats(hash,
                new()
                {
                    Great = stats.Current.Great,
                    MissOther = stats.MissCountHittable + stats.Miss.Block,
                    MissCollectible = stats.MissCountCollectible,
                    Early = stats.Current.Early,
                    Late = stats.Current.Late
                });
        }

        var calcAcc = Math.Round(stats.GetCalculatedAccuracy(), 2);
        if (stats.AccuracyCalculationRest != 0 || Math.Abs(trueAccuracy - calcAcc) > 0.0001)
            stats.OutputCurrentNoteDebuggingData(false);

        if (Config.Advanced.OutputNoteRecordsToDesktop)
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{ModBuildInfo.Name}_Note_Records");
            Directory.CreateDirectory(folder);

            var fileName = $"{MusicInfoUtils.CurMusicName.ToSafeFileName()}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var fullPath = Path.Combine(folder, fileName);

            records.ExportToCsv(fullPath);
        }

        if (!mainConfig.ReplaceResultsScreenMissCount)
            return;

        try
        {
            var pnlVictory = instance.pnlVictory.GetComponentsInChildren<Transform>(true).FirstOrDefault(transform =>
                transform.gameObject.activeSelf &&
                transform.name == "PnlVictory" &&
                transform.gameObject != instance.pnlVictory);

            if (pnlVictory == null)
                throw new NullReferenceException("PnlVictory container not found.");

            var missValueTransform = pnlVictory.Find("PnlVictory_3D/DetailInfo/Other/TxtMiss/TxtValue");
            if (missValueTransform == null)
                throw new NullReferenceException("Miss value text transform not found.");

            var textComponent = missValueTransform.gameObject.GetComponent<UnityEngine.UI.Text>();
            if (textComponent == null)
                throw new NullReferenceException("Text component not found on miss value object.");

            textComponent.text = (stats.MissCountHittable + stats.Miss.Block).ToString();
        }
        catch (Exception ex)
        {
            Log.Error("Replace results screen miss count failed.");
            Log.Error(ex);
        }
    }
}
