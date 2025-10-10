using JetBrains.Annotations;
using MDIP.Application.DependencyInjection;
using MDIP.Application.Services.Global.Configuration;
using MDIP.Application.Services.Scoped.Notes;

namespace MDIP.Presentation.Patches;

[HarmonyPatch(typeof(BaseEnemyObjectController), nameof(BaseEnemyObjectController.ControllerMissCheck))]
internal static class BaseEnemyObjectControllerPatch
{
    private static void Postfix(BaseEnemyObjectController __instance, int i, decimal currentTick)
    {
        if (!ConfigAccessor.Advanced.OutputNoteRecordsToDesktop)
            return;

        NoteRecordService.AddRecord(__instance.m_MusicData, "ControllerMissCheck", $"m_HasMiss:{__instance.m_HasMiss}");
    }

    [UsedImplicitly] [Inject] public static IConfigAccessor ConfigAccessor { get; set; }
    [UsedImplicitly] [Inject] public static INoteRecordService NoteRecordService { get; set; }
}