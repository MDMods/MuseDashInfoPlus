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

        var hash = GameStatsService.PlayingMusicHash;

        var trueAccuracy = Math.Round(GameStatsService.GetTrueAccuracy(), 2);
        var newAcc = (float)trueAccuracy;
        var newScore = GameStatsService.Current.Score;

        var accBest = RuntimeSongDataStore.StorePersonalBestAccuracy(hash, newAcc);
        var scoreBest = RuntimeSongDataStore.StorePersonalBestScore(hash, newScore);

        var mainConfig = ConfigAccessor.Main;
        var newBest = mainConfig.PersonalBestCriteria == 2 ? scoreBest : accBest;

        if (newBest)
        {
            StatsSaverService.SetStats(hash,
                new()
                {
                    Great = GameStatsService.Current.Great,
                    MissOther = GameStatsService.MissCountHittable + GameStatsService.Miss.Block,
                    MissCollectible = GameStatsService.MissCountCollectible,
                    Early = GameStatsService.Current.Early,
                    Late = GameStatsService.Current.Late
                });
        }

        var calcAcc = Math.Round(GameStatsService.GetCalculatedAccuracy(), 2);
        if (GameStatsService.AccuracyCalculationRest != 0 || Math.Abs(trueAccuracy - calcAcc) > 0.0001)
            GameStatsService.OutputCurrentNoteDebuggingData(false);

        if (ConfigAccessor.Advanced.OutputNoteRecordsToDesktop)
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Info+ Note Records");
            Directory.CreateDirectory(folder);

            var fileName = $"{MusicInfoUtils.CurMusicName.ToSafeFileName()}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var fullPath = Path.Combine(folder, fileName);

            NoteRecordService.ExportToCsv(fullPath);
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

            textComponent.text = (GameStatsService.MissCountHittable + GameStatsService.Miss.Block).ToString();
        }
        catch (Exception ex)
        {
            Logger.Error("Replace results screen miss count failed.");
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