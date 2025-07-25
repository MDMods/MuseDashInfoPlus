using Il2CppAssets.Scripts.GameCore.Managers;

namespace MDIP.Patches;

[HarmonyPatch(typeof(StatisticsManager), nameof(StatisticsManager.OnNoteResult))]
internal class StatisticsManagerPatch
{
    private static void Prefix(StatisticsManager __instance, int result)
    {
        if (!Configs.Advanced.OutputNoteRecordsToDesktop) return;

        NoteRecordManager.AddRecord(GameStatsManager.GetCurMusicData(), "OnNoteResult", $"noteResult:{result}");
    }
}