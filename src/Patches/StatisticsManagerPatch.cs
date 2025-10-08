using Il2CppAssets.Scripts.GameCore.Managers;
using JetBrains.Annotations;
using MDIP.Application.Contracts;

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

    #region Injections

    [UsedImplicitly] public static IConfigAccessor ConfigAccessor { get; set; }
    [UsedImplicitly] public static INoteRecordService NoteRecordService { get; set; }
    [UsedImplicitly] public static IGameStatsService GameStatsService { get; set; }

    #endregion
}