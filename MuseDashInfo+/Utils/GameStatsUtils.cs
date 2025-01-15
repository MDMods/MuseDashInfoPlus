using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.GameCore.HostComponent;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppFormulaBase;
using System.Linq;
using MDIP.Modules;
using MDIP.Patches;

namespace MDIP.Utils;

public static class GameStatsUtils
{
    private static StageBattleComponent _stage;
    private static TaskStageTarget _task;

    public static bool Playing => _stage?.isInGame ?? false;

    // Normal judgements
    public static int TotalCount => _stage?.GetMusicData().Count(note => Utils.IsRegularNote(note.noteData.type)) ?? 0;
    public static int HitCount => _task?.m_HitCount ?? 0;
    public static int PerfectCount => _task?.m_PerfectResult ?? 0;
    public static int GreatCount => _task?.m_GreatResult ?? 0;
    public static int MusicNoteCount => _task?.m_MusicCount ?? 0;
    public static int HeartCount => _task?.m_Blood ?? 0;
    public static int MissCount => NormalMissCount + GhostMissCount;
    public static int CurrentScore => _task?.m_Score ?? 0;

    // From patches
    public static int JumpOverCount { get; internal set; }
    public static int NormalMissCount { get; internal set; }
    public static int GhostMissCount { get; internal set; }
    public static int CollectableMissCount { get; internal set; }

    public static int SavedHighestScore { private get; set; } = -1;
    public static int HighestScore { get; private set; } = 0;
    public static int ScoreGap => CurrentScore - HighestScore;

    public static float Accuracy
    {
        get
        {
            var total = PerfectCount + JumpOverCount + MusicNoteCount + HeartCount + GreatCount + NormalMissCount + GhostMissCount + CollectableMissCount;

            if (total == 0)
                return 100;

            var counted = PerfectCount + JumpOverCount + MusicNoteCount + HeartCount + GreatCount * .5f;
            return counted / total * 100;
        }
    }

    public static void DecideHighestScore()
    {
        HighestScore = SavedHighestScore;
        SavedHighestScore = -1;
    }

    public static void LockHighestScore()
    {
        int curHiScore = HighestScore;
        HighestScore = BattleHelper.GetCurrentMusicHighScore() <= 0
            ? curHiScore > 0 ? curHiScore : 0
            : BattleHelper.GetCurrentMusicHighScore();
    }

    public static string GetAccuracyString()
    {
        string color = Accuracy >= 100 ? "#fff000" // SSS
            : Accuracy > 95 ? "#ccf0fe" // SS
            : Accuracy > 90 ? "#ff0089" // S
            : Accuracy > 80 ? "#ad00ff" // A
            : Accuracy > 70 ? "#00bbff" // B
            : Accuracy > 60 ? "#00ff23" // C
            : "#a2a2a2"; // D
        return $"<color={color}>{Accuracy:F2}%</color>";
    }

    public static string GetScoreGapString()
    {
        bool ahead = ScoreGap > 0;
        return $"<color={(ahead ? Constants.GAP_AHEAD_COLOR : Constants.GAP_BEHIND_COLOR)}>{(ahead ? "+" : string.Empty)}{ScoreGap / 1000}K</color>";
    }

    internal static void Reload()
    {
        _stage = Singleton<StageBattleComponent>.instance;
        _task = Singleton<TaskStageTarget>.instance;
    }

    internal static void Reset()
    {
        _stage = null;
        _task = null;

        JumpOverCount = 0;
        NormalMissCount = 0;
        GhostMissCount = 0;
        CollectableMissCount = 0;

        SavedHighestScore = -1;
    }

    public static string GetMissCountsText()
        => (GreatCount + MissCount + CollectableMissCount) == 0 ? "AP"
        : (GreatCount < 1 ? string.Empty : $"{GreatCount}G")
        + (MissCount < 1 ? string.Empty : $" {MissCount}M")
        + ((PnlBattleGameStartPatch.IsSpellMode ? 0 : CollectableMissCount) < 1 ? string.Empty : $" {CollectableMissCount}H");
}

// Reference: https://github.com/flustix/AccDisplay/blob/main/AccDisplay/Utils/GameUtils.cs