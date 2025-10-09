using JetBrains.Annotations;
using MDIP.Application.DependencyInjection;
using MDIP.Application.Services.Configuration;
using MDIP.Application.Services.Notes;

namespace MDIP.Patches;

[HarmonyPatch(typeof(MultHitEnemyController), nameof(MultHitEnemyController.OnControllerMiss))]
internal static class MultHitEnemyControllerPatch
{
    private static void Prefix(MultHitEnemyController __instance, int index)
    {
        if (!ConfigAccessor.Advanced.OutputNoteRecordsToDesktop)
            return;

        NoteRecordService.AddRecord(__instance.m_MusicData, "OnControllerMiss", $"m_HasMiss:{__instance.m_HasMiss}");
    }

    [UsedImplicitly] [Inject] public static IConfigAccessor ConfigAccessor { get; set; }
    [UsedImplicitly] [Inject] public static INoteRecordService NoteRecordService { get; set; }
}