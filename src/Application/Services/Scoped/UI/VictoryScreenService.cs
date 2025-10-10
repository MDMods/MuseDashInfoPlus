using JetBrains.Annotations;
using MDIP.Application.DependencyInjection;
using MDIP.Application.Services.Global.Configuration;
using MDIP.Application.Services.Global.Logging;
using MDIP.Application.Services.Global.RuntimeData;
using MDIP.Application.Services.Global.Stats;
using MDIP.Application.Services.Scoped.Notes;
using MDIP.Application.Services.Scoped.Stats;
using MDIP.Core.Utilities;

namespace MDIP.Application.Services.Scoped.UI;

public class VictoryScreenService : IVictoryScreenService
{
    public void OnSetDetailInfo(PnlVictory instance)
    {
        GameStatsService.IsPlayerPlaying = false;

        var hash = GameUtils.MusicHash;

        var newAcc = (float)Math.Round(GameStatsService.GetCalculatedAccuracy(), 2);
        var newScore = GameStatsService.Current.Score;

        var main = ConfigAccessor.Main;
        var newBest = main.PersonalBestCriteria == 2
            ? RuntimeSongDataStore.StorePersonalBestScore(hash, newScore)
            : RuntimeSongDataStore.StorePersonalBestAccuracy(hash, newAcc);

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
            Logger.Error(ex);
        }
    }

    [UsedImplicitly] [Inject] public IConfigAccessor ConfigAccessor { get; set; }
    [UsedImplicitly] [Inject] public IGameStatsService GameStatsService { get; set; }
    [UsedImplicitly] [Inject] public INoteRecordService NoteRecordService { get; set; }
    [UsedImplicitly] [Inject] public IStatsSaverService StatsSaverService { get; set; }
    [UsedImplicitly] [Inject] public IRuntimeSongDataStore RuntimeSongDataStore { get; set; }
    [UsedImplicitly] [Inject] public ILogger<VictoryScreenService> Logger { get; set; }
}