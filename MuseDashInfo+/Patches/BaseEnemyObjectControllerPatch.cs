using HarmonyLib;
using Il2Cpp;

using MDIP.Managers;

namespace MDIP.Patches;

//[HarmonyPatch(typeof(BaseEnemyObjectController), nameof(BaseEnemyObjectController.ControllerMissCheck))]
public class BaseEnemyObjectControllerPatch
{
    private static void Postfix(BaseEnemyObjectController __instance, int i, Il2CppSystem.Decimal currentTick)
    {
        var note = __instance.m_MusicData;

        NoteRecordManager.AddRecord(int.Parse(note.noteData.id), "ControllerMissCheck", $"m_HasMiss:{__instance.m_HasMiss}");
    }
}
