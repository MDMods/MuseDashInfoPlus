using MDIP.Core.Utilities;
using MDIP.Globals;
using MDIP.Presentation;

namespace MDIP.Battle;

// Handles the results screen: persists this run's great/miss/early/late breakdown to Info+'s own
// StatsStore when it's a new best (the only store that holds the breakdown), and optionally exports
// note records + overwrites the displayed miss count with our hittable-miss count. Owned by a
// BattleSession; reads its siblings GameStats / NoteRecords. "New best" compares this run to the PB
// loaded at battle start (GameStats.History) — the ecosystem itself owns the score/accuracy record.
public class VictoryScreen(GameStats stats, NoteRecords records)
{
    public void OnSetDetailInfo(PnlVictory instance)
    {
        stats.IsPlayerPlaying = false;

        var trueAccuracy = (float)Math.Round(stats.GetTrueAccuracy(), 2);
        var newScore = stats.Current.Score;

        var mainConfig = Config.Main;
        // New best vs the PB loaded at battle start. A first play (no PB) compares against 0 → a run
        // with any score/accuracy counts as a new best, which is what we want.
        var newBest = mainConfig.PersonalBestCriteria == 2
            ? newScore > stats.History.Score
            : trueAccuracy > stats.History.Accuracy;

        if (newBest)
        {
            StatsStore.SetStats(stats.PlayingMusicHash,
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
            records.ExportToCsv(Path.Combine(folder, fileName));
        }

        if (!mainConfig.ReplaceResultsScreenMissCount)
            return;

        // Overwrite the displayed miss count via the results panel's typed controls — no hierarchy walk.
        var controls = instance.m_CurControls;
        if (controls?.missTxt == null)
            return;

        var missText = (stats.MissCountHittable + stats.Miss.Block).ToString();
        foreach (var txt in controls.missTxt)
            if (txt != null)
                txt.text = missText;
    }
}
