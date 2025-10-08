using JetBrains.Annotations;
using MDIP.Application.Contracts;

namespace MDIP.Patches;

[HarmonyPatch(typeof(BaseEnemyObjectController), nameof(BaseEnemyObjectController.ControllerMissCheck))]
internal static class BaseEnemyObjectControllerPatch
{
    private static void Postfix(BaseEnemyObjectController __instance, int i, decimal currentTick)
    {
        if (!ConfigAccessor.Advanced.OutputNoteRecordsToDesktop)
            return;

        NoteRecordService.AddRecord(__instance.m_MusicData, "ControllerMissCheck", $"m_HasMiss:{__instance.m_HasMiss}");
    }

    #region Injections

    [UsedImplicitly] public static IConfigAccessor ConfigAccessor { get; set; }
    [UsedImplicitly] public static INoteRecordService NoteRecordService { get; set; }

    #endregion
}