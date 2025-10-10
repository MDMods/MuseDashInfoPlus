using Il2CppAssets.Scripts.GameCore.Managers;
using JetBrains.Annotations;
using MDIP.Application.DependencyInjection;
using MDIP.Application.Services.Global.Configuration;
using MDIP.Application.Services.Scoped.Notes;
using MDIP.Application.Services.Scoped.Stats;

namespace MDIP.Presentation.Patches;

[HarmonyPatch(typeof(StatisticsManager), nameof(StatisticsManager.OnNoteResult))]
internal static class StatisticsManagerPatch
{
    private static void Prefix(StatisticsManager __instance, int result)
    {
        if (!ConfigAccessor.Advanced.OutputNoteRecordsToDesktop)
            return;

        NoteRecordService.AddRecord(GameStatsService.GetCurMusicData(), "OnNoteResult", $"noteResult:{result}");
    }

    [UsedImplicitly] [Inject] public static IConfigAccessor ConfigAccessor { get; set; }
    [UsedImplicitly] [Inject] public static INoteRecordService NoteRecordService { get; set; }
    [UsedImplicitly] [Inject] public static IGameStatsService GameStatsService { get; set; }
}