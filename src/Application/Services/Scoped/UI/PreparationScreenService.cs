using JetBrains.Annotations;
using MDIP.Application.DependencyInjection;
using MDIP.Application.Services.Global.RuntimeData;
using MDIP.Application.Services.Scoped.Stats;
using MDIP.Core.Utilities;

namespace MDIP.Application.Services.Scoped.UI;

public class PreparationScreenService : IPreparationScreenService
{
    public void OnRecordUpdated(PnlPreparation instance)
    {
        if (instance?.pnlRecord == null)
            return;

        RuntimeSongDataStore.StorePersonalBestAccuracyFromText(MusicInfoUtils.CurMusicHash, instance.pnlRecord.txtAccuracy?.m_Text);
        RuntimeSongDataStore.StorePersonalBestScoreFromText(MusicInfoUtils.CurMusicHash, instance.pnlRecord.txtScore?.m_Text);
    }

    [UsedImplicitly] [Inject] public IRuntimeSongDataStore RuntimeSongDataStore { get; set; }
}