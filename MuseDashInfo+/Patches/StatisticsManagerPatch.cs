using Il2CppAssets.Scripts.GameCore.Managers;

namespace MDIP.Patches;

[HarmonyPatch(typeof(StatisticsManager), nameof(StatisticsManager.OnNoteResult))]
public class StatisticsManagerPatch
{
    private static void Prefix(StatisticsManager __instance, int result)
    {
        if (!Configs.Advanced.OutputNoteRecordsToDesktop) return;

        var note = GameStatsManager.GetCurMusicData();
        NoteRecordManager.AddRecord(int.Parse(note.noteData.id), "OnNoteResult", $"result:{result}");
    }
}