using Il2CppAssets.Scripts.GameCore.Managers;
using JetBrains.Annotations;
using MDIP.Application.Services.Configuration;
using MDIP.Application.Services.Notes;
using MDIP.Application.Services.Stats;

namespace MDIP.Patches;

[HarmonyPatch(typeof(StatisticsManager), nameof(StatisticsManager.OnNoteResult))]
internal static class StatisticsManagerPatch
{
    private static void Prefix(StatisticsManager __instance, int result)
    {
        if (!ConfigAccessor.Advanced.OutputNoteRecordsToDesktop)
            return;

        NoteRecordService.AddRecord(GameStatsService.GetCurMusicData(), "OnNoteResult", $"noteResult:{result}");
    }

    [UsedImplicitly] public static IConfigAccessor ConfigAccessor { get; set; }
    [UsedImplicitly] public static INoteRecordService NoteRecordService { get; set; }
    [UsedImplicitly] public static IGameStatsService GameStatsService { get; set; }
}