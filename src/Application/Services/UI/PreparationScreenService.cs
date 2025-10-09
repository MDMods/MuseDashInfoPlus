using JetBrains.Annotations;
using MDIP.Application.Services.Stats;

namespace MDIP.Application.Services.UI;

public class PreparationScreenService : IPreparationScreenService
{
    public void OnRecordUpdated(PnlPreparation instance)
    {
        if (instance?.pnlRecord == null)
            return;

        GameStatsService.StoreHighestAccuracyFromText(instance.pnlRecord.txtAccuracy?.m_Text);
        GameStatsService.StoreHighestScoreFromText(instance.pnlRecord.txtScore?.m_Text);
    }

    [UsedImplicitly] public required IGameStatsService GameStatsService { get; init; }
}