namespace MDIP.Patches;

[HarmonyPatch(typeof(MultHitEnemyController), nameof(MultHitEnemyController.OnControllerMiss))]
internal class MultHitEnemyControllerPatch
{
    private static void Prefix(MultHitEnemyController __instance, int index)
    {
        if (!Configs.Advanced.OutputNoteRecordsToDesktop) return;

        NoteRecordManager.AddRecord(__instance.m_MusicData, "OnControllerMiss", $"m_HasMiss:{__instance.m_HasMiss}");
    }
}