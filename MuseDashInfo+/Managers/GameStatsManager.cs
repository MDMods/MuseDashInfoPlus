using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.GameCore.HostComponent;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppFormulaBase;
using System.Linq;

using MDIP.Utils;
using MDIP.Patches;
using System.Collections.Generic;

namespace MDIP.Managers;

public static class GameStatsManager
{
    private static StageBattleComponent _stage;
    private static TaskStageTarget _task;

    // From vanilla
    public static int VanillaCurrentScore => _task?.m_Score ?? 0;
    public static int VanillaHitCount => _task?.m_HitCount ?? 0;
    public static int VanillaPerfectCount => _task?.m_PerfectResult ?? 0;
    public static int VanillaGreatCount => _task?.m_GreatResult ?? 0;
    public static int VanillaMusicNoteCount => _task?.m_MusicCount ?? 0;
    public static int VanillaHeartCount => _task?.m_Blood ?? 0;

    // From mod
    public static int JumpOverCount { get; private set; }
    public static int NormalMissCount { get; private set; }
    public static int GhostMissCount { get; private set; }
    public static int HeartMissCount { get; private set; }
    public static int MusicNoteMissCount { get; private set; }

    public static int CurrentScoreMissCount => NormalMissCount + GhostMissCount;
    public static int CollectableMissCount => HeartMissCount + MusicNoteMissCount;
    public static int TotalMissCount => NormalMissCount + GhostMissCount + CollectableMissCount;
    public static int TotalCollectableCount => VanillaHeartCount + VanillaMusicNoteCount;
    public static int TotalCountedCount => VanillaPerfectCount + VanillaGreatCount + JumpOverCount + TotalCollectableCount;

    public static int TotalNoteCount { get; private set; }
    public static int SavedHighestScore { private get; set; }
    public static int HighestScore { get; private set; }
    public static int ScoreGap => VanillaCurrentScore - HighestScore;

    private static List<int> CountedNoteIdList = new();
    private static List<int> CountedBindHeartList = new();

    public static float GetAccuracy()
    {
        int total = TotalCountedCount + TotalMissCount;
        return total <= 0 ? 100
            : VanillaPerfectCount + VanillaGreatCount * .5f + JumpOverCount + TotalCollectableCount / total * 100;
    }

    public static string GetAccuracyString()
    {
        var acc = GetAccuracy();
        string color = acc >= 100 ? "#fff000" // SSS
            : acc > 95 ? "#ccf0fe" // SS
            : acc > 90 ? "#ff0089" // S
            : acc > 80 ? "#ad00ff" // A
            : acc > 70 ? "#00bbff" // B
            : acc > 60 ? "#00ff23" // C
            : "#a2a2a2"; // D
        return $"<color={color}>{acc:F2}%</color>";
    }

    public static string GetScoreGapString()
    {
        bool ahead = ScoreGap > 0;
        return $"<color={(ahead ? Constants.GAP_AHEAD_COLOR : Constants.GAP_BEHIND_COLOR)}>{(ahead ? "+" : string.Empty)}{ScoreGap / 1000}K</color>";
    }

    public static string GetMissCountsString()
        => (VanillaGreatCount + TotalMissCount) == 0 ? "AP"
        : (VanillaGreatCount < 1 ? string.Empty : $"{VanillaGreatCount}G")
        + ((NormalMissCount + GhostMissCount) < 1 ? string.Empty : $" {NormalMissCount + GhostMissCount}M")
        + ((PnlBattleGameStartPatch.IsSpellMode ? 0 : CollectableMissCount) < 1 ? string.Empty : $" {CollectableMissCount}H");

    public static void LockHighestScore()
    {
        HighestScore = SavedHighestScore;
        SavedHighestScore = -1;
    }

    public static void DecideConstantDatas()
    {
        int curHiScore = HighestScore;
        HighestScore = BattleHelper.GetCurrentMusicHighScore() <= 0
            ? curHiScore > 0 ? curHiScore : 0
            : BattleHelper.GetCurrentMusicHighScore();

        TotalNoteCount = _stage?.GetMusicData()?.Count(Utils.Utils.IsSingleNoteFunc) ?? 0;
    }

    public static void AddNormalMiss(int id, int doubleId = -1)
    {
        if (CountedNoteIdList.Contains(id) || CountedNoteIdList.Contains(doubleId)) return;
        if (doubleId != -1)
        {
            // Double counted as 2 misses
            // And ignore if there is an another call in
            CountedNoteIdList.Add(doubleId);
            NormalMissCount++;
        }
        CountedNoteIdList.Add(id);
        NormalMissCount++;
    }

    public static void AddGhostMiss(int id)
    {
        if (CountedNoteIdList.Contains(id)) return;
        CountedNoteIdList.Add(id);
        GhostMissCount++;
    }

    public static void AddHeartMiss(int id)
    {
        if (CountedNoteIdList.Contains(id)) return;
        CountedNoteIdList.Add(id);
        HeartMissCount++;
    }

    public static void AddBindHeartMiss(int id)
    {
        if (CountedBindHeartList.Contains(id)) return;
        CountedBindHeartList.Add(id);
        HeartMissCount++;
    }

    public static void AddMusicNoteMiss(int id)
    {
        if (CountedNoteIdList.Contains(id)) return;
        CountedNoteIdList.Add(id);
        MusicNoteMissCount++;
    }

    public static void AddJumpOver(int id)
    {
        if (CountedNoteIdList.Contains(id)) return;
        CountedNoteIdList.Add(id);
        JumpOverCount++;
    }

    public static void Reload()
    {
        _stage = Singleton<StageBattleComponent>.instance;
        _task = Singleton<TaskStageTarget>.instance;
    }

    public static void Reset()
    {
        _stage = null;
        _task = null;

        JumpOverCount = 0;
        NormalMissCount = 0;
        GhostMissCount = 0;
        HeartMissCount = 0;
        MusicNoteMissCount = 0;

        TotalNoteCount = -1;
        SavedHighestScore = -1;

        CountedNoteIdList = new();
        CountedBindHeartList = new();
    }
}

// Reference: https://github.com/flustix/AccDisplay/blob/main/AccDisplay/Utils/GameUtils.cs