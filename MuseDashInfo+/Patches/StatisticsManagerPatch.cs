using HarmonyLib;
using Il2CppAssets.Scripts.GameCore.Managers;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppFormulaBase;

using MDIP.Managers;

namespace MDIP.Patches;

[HarmonyPatch(typeof(StatisticsManager), nameof(StatisticsManager.OnNoteResult))]
public class StatisticsManagerPatch
{
    private static void Prefix(StatisticsManager __instance, int result)
    {
        if (!Utils.Helper.OutputNoteRecordsToDesktop) return;

        var note = Singleton<StageBattleComponent>.instance.GetCurMusicData();
        NoteRecordManager.AddRecord(int.Parse(note.noteData.id), "OnNoteResult", $"result:{result}");
    }
}