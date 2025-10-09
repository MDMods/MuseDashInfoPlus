using JetBrains.Annotations;
using MDIP.Application.Services.Configuration;
using MDIP.Application.Services.Logging;
using MDIP.Application.Services.Notes;
using MDIP.Application.Services.Stats;
using MDIP.Utils;

namespace MDIP.Application.Services.UI;

public class VictoryScreenService : IVictoryScreenService
{
    public void OnSetDetailInfo(PnlVictory instance)
    {
        GameStatsService.IsInGame = false;
        GameStatsService.IsFirstTry = false;

        var newAcc = (float)Math.Round(GameStatsService.GetCalculatedAccuracy(), 2);
        var newScore = GameStatsService.Current.Score;

        var main = ConfigAccessor.Main;
        var newBest = main.PersonalBestCriteria == 2
            ? newScore >= GameStatsService.StoredHighestScore
            : newAcc >= GameStatsService.StoredHighestAccuracy;

        GameStatsService.StoreHighestAccuracy(newAcc);
        GameStatsService.StoreHighestScore(newScore);

        if (newBest)
        {
            StatsSaverService.SetStats(GameUtils.MusicHash,
                new()
                {
                    Great = GameStatsService.Current.Great,
                    MissOther = GameStatsService.MissCountHittable + GameStatsService.Miss.Block,
                    MissCollectible = GameStatsService.MissCountCollectible,
                    Early = GameStatsService.Current.Early,
                    Late = GameStatsService.Current.Late
                });
        }

        var trueAcc = Math.Round(GameStatsService.GetTrueAccuracy(), 2);
        var calcAcc = Math.Round(GameStatsService.GetCalculatedAccuracy(), 2);
        if (GameStatsService.AccuracyCalculationRest != 0 || Math.Abs(trueAcc - calcAcc) > 0.0001f)
            GameStatsService.OutputCurrentNoteDebuggingData(false);

        if (ConfigAccessor.Advanced.OutputNoteRecordsToDesktop)
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Info+ Note Records");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            NoteRecordService.ExportToCsv(Path.Combine(folder, $"{GameUtils.MusicName}.csv"));
        }

        if (!main.ReplaceResultsScreenMissCount)
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
                .gameObject.GetComponent<UnityEngine.UI.Text>()
                .text = (GameStatsService.MissCountHittable + GameStatsService.Miss.Block).ToString();
        }
        catch (Exception ex)
        {
            Logger.Error(ex.ToString());
        }
    }

    [UsedImplicitly] public IConfigAccessor ConfigAccessor { get; set; }
    [UsedImplicitly] public IGameStatsService GameStatsService { get; set; }
    [UsedImplicitly] public INoteRecordService NoteRecordService { get; set; }
    [UsedImplicitly] public IStatsSaverService StatsSaverService { get; set; }
    [UsedImplicitly] public ILogger<VictoryScreenService> Logger { get; set; }
}