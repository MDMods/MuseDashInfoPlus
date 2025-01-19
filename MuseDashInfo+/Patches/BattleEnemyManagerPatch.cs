using Il2CppAssets.Scripts.GameCore.HostComponent;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppFormulaBase;
using HarmonyLib;

using MDIP.Modules;
using MDIP.Manager;
using MDIP.Managers;

namespace MDIP.Patches;

[HarmonyPatch(typeof(BattleEnemyManager), nameof(BattleEnemyManager.SetPlayResult))]
public class BattleEnemyManagerSetPlayResultPatch
{
    private static void Postfix(int idx, byte result, bool isMulStart = false, bool isMulEnd = false, bool isLeft = false)
    {
        var note = Singleton<StageBattleComponent>.instance.GetMusicDataByIdx(idx);
        var type = (NoteType)note.noteData.type;

        //NoteRecordManager.AddRecord(idx, "SetPlayResult", $"result:{result}");

        switch (result)
        {
            case 4 when type == NoteType.Block:
                GameStatsManager.AddBlockCur(idx);
                break;
            case 1 when type == NoteType.Long:
                GameStatsManager.AddLongMiss(idx, note.isLongPressStart);
                break;
        }

        StatsTextManager.UpdateAllText();
    }
}

// Reference: https://github.com/flustix/AccDisplay/blob/main/AccDisplay/Patches/MissPatches.cs