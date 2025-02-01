using Il2Cpp;

namespace MDIP.Patches;

[HarmonyPatch(typeof(BaseEnemyObjectController), nameof(BaseEnemyObjectController.ControllerMissCheck))]
internal class BaseEnemyObjectControllerPatch
{
	private static void Postfix(BaseEnemyObjectController __instance, int i, decimal currentTick)
	{
		if (!Configs.Advanced.OutputNoteRecordsToDesktop) return;

		var note = __instance.m_MusicData;
		NoteRecordManager.AddRecord(int.Parse(note.noteData.id), "ControllerMissCheck", $"m_HasMiss:{__instance.m_HasMiss}");
	}
}