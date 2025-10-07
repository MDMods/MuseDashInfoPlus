namespace MDIP.Managers;

public static class PreparationScreenManager
{
    public static void OnRecordUpdated(PnlPreparation instance)
    {
        if (instance?.pnlRecord == null) return;
        GameStatsManager.StoreHighestAccuracyFromText(instance.pnlRecord.txtAccuracy?.m_Text);
        GameStatsManager.StoreHighestScoreFromText(instance.pnlRecord.txtScore?.m_Text);
    }
}