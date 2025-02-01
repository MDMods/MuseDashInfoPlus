using Il2Cpp;

namespace MDIP.Patches;

[HarmonyPatch(typeof(MultHitEnemyController), nameof(MultHitEnemyController.OnControllerMiss))]
public class MultHitEnemyControllerPatch
{
	private static void Prefix(MultHitEnemyController __instance, int index)
	{
		if (!Helper.OutputNoteRecordsToDesktop) return;

		var note = __instance.m_MusicData;
		NoteRecordManager.AddRecord(int.Parse(note.noteData.id), "OnControllerMiss", $"m_HasMiss:{__instance.m_HasMiss}");
	}
}