namespace MDIP.Patches;

[HarmonyPatch(typeof(BaseEnemyObjectController), nameof(BaseEnemyObjectController.ControllerMissCheck))]
internal class BaseEnemyObjectControllerPatch
{
    private static void Postfix(BaseEnemyObjectController __instance, int i, decimal currentTick)
    {
        if (!Configs.Advanced.OutputNoteRecordsToDesktop) return;

        NoteRecordManager.AddRecord(__instance.m_MusicData, "ControllerMissCheck", $"m_HasMiss:{__instance.m_HasMiss}");
    }
}