using MDIP.Core.Utilities;

namespace MDIP.Globals;

// Scrapes the preparation screen's personal-best score/accuracy text into RuntimeData, so the
// in-battle overlay can show a PB for the current song. Works transparently for any ecosystem that
// populates the native record panel (vanilla, CustomAlbums); Euterpe charts are seeded via the
// reflection bridge in ChartSource instead.
internal static class PrepScreen
{
    public static void OnRecordUpdated(PnlPreparation instance)
    {
        if (instance?.pnlRecord == null)
            return;

        RuntimeData.StorePersonalBestAccuracyFromText(MusicInfoUtils.CurMusicHash, instance.pnlRecord.txtAccuracy?.m_Text);
        RuntimeData.StorePersonalBestScoreFromText(MusicInfoUtils.CurMusicHash, instance.pnlRecord.txtScore?.m_Text);
    }
}
