using JetBrains.Annotations;
using MDIP.Application.DependencyInjection;
using MDIP.Application.Services.Global.RuntimeData;
using MDIP.Application.Services.Scoped.UI;
using MDIP.Core.Utilities;

namespace MDIP.Application.Services.Global.UI;

public class PreparationScreenService : IPreparationScreenService
{
    public void OnRecordUpdated(PnlPreparation instance)
    {
        if (instance?.pnlRecord == null)
            return;

        RuntimeDataStore.StorePersonalBestAccuracyFromText(MusicInfoUtils.CurMusicHash, instance.pnlRecord.txtAccuracy?.m_Text);
        RuntimeDataStore.StorePersonalBestScoreFromText(MusicInfoUtils.CurMusicHash, instance.pnlRecord.txtScore?.m_Text);
    }

    [UsedImplicitly] [Inject] public IRuntimeDataStore RuntimeDataStore { get; set; }
}